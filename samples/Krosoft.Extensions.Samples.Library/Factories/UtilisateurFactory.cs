using Krosoft.Extensions.Samples.Library.Models;

namespace Krosoft.Extensions.Samples.Library.Factories;

public static class UtilisateurFactory
{
    public static UtilisateurBasique CreateUtilisateur(string nom, string prenom) =>
        new UtilisateurBasique
        {
            Nom = nom,
            Prenom = prenom,
            DateModification = DateTime.Now
        };
}