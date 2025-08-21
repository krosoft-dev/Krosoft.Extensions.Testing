namespace Krosoft.Extensions.Samples.Library.Models;

public sealed class UtilisateurBasique : IEquatable<UtilisateurBasique>
{
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public DateTime DateModification { get; set; }

    public bool Equals(UtilisateurBasique? other) =>
        other != null &&
        Nom == other.Nom &&
        Prenom == other.Prenom;

    public override bool Equals(object? obj) => Equals(obj as UtilisateurBasique);

    public override int GetHashCode() => HashCode.Combine(Nom, Prenom);
}