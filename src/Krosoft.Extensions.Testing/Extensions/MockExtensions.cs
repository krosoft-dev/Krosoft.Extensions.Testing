using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Krosoft.Extensions.Testing.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NFluent;
using NFluent.Extensibility;
using NFluent.Kernel;

namespace Krosoft.Extensions.Testing.Extensions;

public static class MockExtensions
{
    private static IEnumerable<KeyValuePair<string, object?>> GetRow<T>(T item,
                                                                        PropertyInfo[] properties)
    {
        var row = new List<KeyValuePair<string, object?>>();
        foreach (var propertyInfo in properties)
        {
            var value = Convert.ChangeType(propertyInfo.GetValue(item), typeof(object));

            var keyValuePair = new KeyValuePair<string, object?>(propertyInfo.Name, value);
            row.Add(keyValuePair);
        }

        return row;
    }

    public static Mock<IDbCommand> SetupWithData<T>(this Mock<IDbCommand> mock,
                                                    IEnumerable<T> collection)
    {
        var mockDataReader = new Mock<IDataReader>(MockBehavior.Strict);
        mockDataReader.SetupWithData(collection);

        var moqConnection = new Mock<IDbConnection>(MockBehavior.Strict);
        moqConnection.Setup(x => x.Open());
        moqConnection.Setup(x => x.Close());
        moqConnection.Setup(x => x.Dispose());
        moqConnection.Setup(x => x.State).Returns(ConnectionState.Closed);
        moqConnection.Setup(x => x.State).Returns(ConnectionState.Broken);

        mock.Setup(m => m.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(mockDataReader.Object);

        mock.Setup(m => m.ExecuteReader())
            .Returns(mockDataReader.Object);

        mock.Setup(m => m.Connection)
            .Returns(moqConnection.Object);

        return mock;
    }

    public static Mock<IDataReader> SetupWithData<T>(this Mock<IDataReader> mock,
                                                     IEnumerable<T> collection)
    {
        mock.Setup(x => x.Dispose());

        IEnumerable<IEnumerable<KeyValuePair<string, object?>>> data;
        if (collection is
            IEnumerable<IEnumerable<KeyValuePair<string, object?>>>
           )
        {
            data = (IEnumerable<IEnumerable<KeyValuePair<string, object?>>>)collection;
        }
        else
        {
            data = ToData(collection);
        }

        var dataReaderDescriptor = new DataReaderDescriptor(data);
        mock.SetupWithData(dataReaderDescriptor);

        return mock;
    }

    private static void SetupWithData(this Mock<IDataReader> mock,
                                      DataReaderDescriptor dataReaderDescriptor)
    {
        var row = -1;
        mock.Setup(r => r.FieldCount).Returns(dataReaderDescriptor.FieldCount);

        mock.Setup(r => r[It.IsAny<string>()]).Returns((string name) => dataReaderDescriptor.GetValue<object>(row, dataReaderDescriptor.GetOrdinal(name)) ?? "");
        mock.Setup(r => r[It.IsAny<int>()]).Returns((int ordinal) => dataReaderDescriptor.GetValue<object>(row, ordinal) ?? "");

        mock.Setup(r => r.NextResult()).Returns(false);
        mock.Setup(r => r.Read())
            .Returns(() => row < dataReaderDescriptor.Data.Count() - 1)
            .Callback(() => { row++; });

        mock.Setup(r => r.Close()).Callback(() => dataReaderDescriptor.Closed = true);
        mock.Setup(r => r.IsClosed).Returns(() => dataReaderDescriptor.Closed);

        mock.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns<string>(dataReaderDescriptor.GetOrdinal);
        mock.Setup(r => r.GetValues(It.IsAny<object[]>())).Returns<object[]>(values => dataReaderDescriptor.GetValues(row, values));

        mock.Setup(r => r.GetName(It.IsAny<int>())).Returns<int>(dataReaderDescriptor.GetName);
        mock.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<object?>(row, col) == null);
        mock.Setup(r => r.GetValue(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<object>(row, col) ?? "");
        mock.Setup(r => r.GetBoolean(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<bool>(row, col));
        mock.Setup(r => r.GetByte(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<byte>(row, col));
        mock.Setup(r => r.GetChar(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<char>(row, col));
        mock.Setup(r => r.GetDateTime(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<DateTime>(row, col));
        mock.Setup(r => r.GetDecimal(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<decimal>(row, col));
        mock.Setup(r => r.GetDouble(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<double>(row, col));
        mock.Setup(r => r.GetFloat(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<float>(row, col));
        mock.Setup(r => r.GetGuid(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<Guid>(row, col));
        mock.Setup(r => r.GetInt16(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<short>(row, col));
        mock.Setup(r => r.GetInt32(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<int>(row, col));
        mock.Setup(r => r.GetInt64(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<long>(row, col));
        mock.Setup(r => r.GetString(It.IsAny<int>())).Returns<int>(col => dataReaderDescriptor.GetValue<string>(row, col) ?? "");

        Expression<Func<int, bool>> outOfRange = i => i < 0 || i >= dataReaderDescriptor.FieldCount;
        mock.Setup(r => r.GetName(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetFieldType(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetValue(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetBoolean(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetByte(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetChar(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetDataTypeName(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetDateTime(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetDecimal(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetDouble(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetFieldType(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetFloat(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetGuid(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetInt16(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetInt32(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetInt64(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
        mock.Setup(r => r.GetString(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
    }

    private static IEnumerable<IEnumerable<KeyValuePair<string, object?>>> ToData<T>(IEnumerable<T> collection)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var data = new List<IEnumerable<KeyValuePair<string, object?>>>();

        foreach (var item in collection)
        {
            var row = GetRow(item, properties);
            data.Add(row);
        }

        return data;
    }

    public static void Verify<T>(this Mock<ILogger<T>> mock, LogLevel level, string message, Times times)
    {
        mock.Verify(x => x.Log(level,
                               It.IsAny<EventId>(),
                               It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains(message)),
                               It.IsAny<Exception>(),
                               ((Func<It.IsAnyType, Exception, string>)It.IsAny<object>())!),
                    times);
    }

    public static ICheck<Mock<T>> Verify<T>(this ICheck<Mock<T>> check,
                                            Expression<Action<T>> expression,
                                            Func<Times> times,
                                            string? failMessage = null)
        where T : class
    {
        var runnableCheck = ExtensibilityHelper.ExtractChecker(check);

        var value = runnableCheck.Value;
        var executeCheck = runnableCheck.ExecuteCheck(() =>
        {
            try
            {
                if (failMessage != null)
                {
                    value.Verify(expression, times, failMessage);
                }
                else
                {
                    value.Verify(expression, times);
                }
            }
            catch (MockException e)
            {
                throw new FluentCheckException(e.Message);
            }
        }, string.Empty);

        return executeCheck.And;
    }

    public static void VerifyWasCalled<T>(this Mock<ILogger<T>> fakeLogger, LogLevel logLevel, string message, Times times)
    {
        fakeLogger.Verify(x => x.Log(logLevel,
                                     It.IsAny<EventId>(),
                                     It.Is<It.IsAnyType>((o, t) => o != null && !string.IsNullOrEmpty(o.ToString()) && o.ToString()!.StartsWith(message)),
                                     It.IsAny<Exception>(),
                                     It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                          times);
    }
}