using FluentValidation;
using Krosoft.Extensions.Core.Models;
using Krosoft.Extensions.Core.Models.Exceptions;
using Krosoft.Extensions.Samples.Library.Models.Entities;
using Krosoft.Extensions.Testing;
using Krosoft.Extensions.Validations.Extensions;
using Krosoft.Extensions.WebApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Krosoft.Extensions.Validations.Tests.Extensions;

[TestClass]
public class ValidatorsExtensionsTests : BaseTest
{
    private IEnumerable<IValidator<SampleEntity>> _validators = null!;

    protected override void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddWebApi(configuration, typeof(SampleEntity).Assembly);
    }

    [TestInitialize]
    public void SetUp()
    {
        var serviceProvider = CreateServiceCollection();
        _validators = serviceProvider.GetRequiredService<IEnumerable<IValidator<SampleEntity>>>();

        Check.That(_validators).HasSize(1);
    }

    [TestMethod]
    public void ValidateAndThrowAsync_Ok()
    {
        Check.ThatCode(() => _validators.ValidateMoreAndThrowAsync(new SampleEntity(), CancellationToken.None))
             .Throws<KrosoftFunctionalException>()
             .WithMessage("'Id' ne doit pas être vide.");
    }

    [TestMethod]
    public async Task ValidateMoreAsync_Empty()
    {
        var failures = await _validators.ValidateMoreAsync(new SampleEntity
        {
            Id = 1,
            Name = "Herllo"
        }, CancellationToken.None);

        Check.That(failures).IsEmpty();
    }

    [TestMethod]
    public async Task ValidateMoreAsync_Ok()
    {
        var errors = await _validators.ValidateMoreAsync(new SampleEntity(), CancellationToken.None);

        Check.That(errors).IsNotNull();
        Check.That(errors).HasSize(2);
        var details = errors.ToList();

        Check.That(details[0].TypeName).IsEqualTo("SampleEntity");
        Check.That(details[0].PropertyName).IsEqualTo("Id");
        Check.That(details[0].Errors).ContainsExactly("'Id' ne doit pas être vide.");
        Check.That(details[1].TypeName).IsEqualTo("SampleEntity");
        Check.That(details[1].PropertyName).IsEqualTo("Name");
        Check.That(details[1].Errors).ContainsExactly("'Name' ne doit pas être vide.", "'Name' ne doit pas avoir la valeur null.");
    }

    [TestMethod]
    public async Task ValidateMoreAsync_WithAction_Empty()
    {
        IEnumerable<ErrorDetail>? f = null;
        await _validators.ValidateMoreAsync(new SampleEntity
        {
            Id = 1,
            Name = "Herllo"
        }, failures => { f = failures; }, CancellationToken.None);

        Check.That(f).IsEmpty();
    }

    [TestMethod]
    public async Task ValidateMoreAsync_WithAction_Ok()
    {
        ISet<ErrorDetail>? errors = null;
        await _validators.ValidateMoreAsync(new SampleEntity(), failures => { errors = failures; }, CancellationToken.None);

        Check.That(errors).IsNotNull();
        Check.That(errors).HasSize(2);
        var details = errors?.ToList()!;

        Check.That(details[0].TypeName).IsEqualTo("SampleEntity");
        Check.That(details[0].PropertyName).IsEqualTo("Id");
        Check.That(details[0].Errors).ContainsExactly("'Id' ne doit pas être vide.");
        Check.That(details[1].TypeName).IsEqualTo("SampleEntity");
        Check.That(details[1].PropertyName).IsEqualTo("Name");
        Check.That(details[1].Errors).ContainsExactly("'Name' ne doit pas être vide.", "'Name' ne doit pas avoir la valeur null.");
    }
}