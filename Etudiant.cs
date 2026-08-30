using System;

namespace GestionClasse
{
    public class Etudiant
    {
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Matricule { get; set; }
        public double? Note { get; set; }

        public Etudiant(string nom, string prenom, string matricule)
        {
            Nom = nom;
            Prenom = prenom;
            Matricule = matricule;
            Note = null;
        }

        public string ObtenirMention()
        {
            if (!Note.HasValue) return "Non noté";

            double n = Note.Value;
            if (n >= 16) return "Très bien";
            if (n >= 14) return "Bien";
            if (n >= 12) return "Assez bien";
            if (n >= 10) return "Passable";
            return "Insuffisant";
        }

        public override string ToString()
        {
            string noteAffichage = Note.HasValue ? $"{Note.Value:F2}/20" : "Non renseignée";
            return $"Matricule: {Matricule,-10} | Nom: {Nom,-12} | Prénom: {Prenom,-12} | Note: {noteAffichage,-14} | Mention: {ObtenirMention()}";
        }
    }
}