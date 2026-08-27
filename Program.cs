using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GestionClasse
{
    // ==========================================
    // 1. DÉFINITION DE LA CLASSE ETUDIANT
    // ==========================================
    public class Etudiant
    {
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Matricule { get; set; }
        public double? Note { get; set; } // Note non renseignée (null) par défaut

        public Etudiant(string nom, string prenom, string matricule)
        {
            Nom = nom;
            Prenom = prenom;
            Matricule = matricule;
            Note = null;
        }

        // Calcule la mention selon la grille du sujet
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
            return $"Matricule: {Matricule} | Nom: {Nom} | Prénom: {Prenom} | Note: {noteAffichage} | Mention: {ObtenirMention()}";
        }
    }

    // ==========================================
    // BONUS : CLASSE POUR GÉRER UNE CLASSE (ex: 3A, L1)
    // ==========================================
    public class Classe
    {
        public string NomClasse { get; set; }
        public List<Etudiant> Etudiants { get; set; }

        public Classe(string nomClasse)
        {
            NomClasse = nomClasse;
            Etudiants = new List<Etudiant>();
        }
    }

    // ==========================================
    // 2. PROGRAMME PRINCIPAL
    // ==========================================
    class Program
    {
        // Liste globale des classes
        static List<Classe> listeClasses = new List<Classe>();
        static Classe classeActive = null;

        static void Main(string[] args)
        {
            // Initialisation par défaut avec une première classe si aucune n'existe
            Console.Clear();
            Console.WriteLine("=== INITIALISATION DU SYSTÈME ===");
            Console.Write("Entrez le nom de la première classe à gérer (ex: L1, 3A, L2): ");
            string nomInit = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(nomInit)) nomInit = "L1";

            Classe premiereClasse = new Classe(nomInit);
            listeClasses.Add(premiereClasse);
            classeActive = premiereClasse;

            bool continuer = true;

            while (continuer)
            {
                Console.Clear();
                Console.WriteLine($"=== GESTION DE CLASSE - Classe active : [{classeActive.NomClasse}] ===");
                Console.WriteLine("1. Saisir les étudiants");
                Console.WriteLine("2. Saisir / Modifier les notes");
                Console.WriteLine("3. Afficher la liste complète");
                Console.WriteLine("4. Afficher les étudiants admis");
                Console.WriteLine("5. Afficher les étudiants à rattraper");
                Console.WriteLine("6. Rechercher un étudiant");
                Console.WriteLine("7. Afficher les statistiques de la classe (Moyenne + Médiane)");
                Console.WriteLine("8. Trier les étudiants (par nom ou par note)");
                Console.WriteLine("9. Supprimer un étudiant");
                Console.WriteLine("10. Exporter la liste des admis (Fichier .txt)");
                Console.WriteLine("11. Gérer / Changer de classe (Gestion multi-classes)");
                Console.WriteLine("12. Quitter");
                Console.Write("\nChoisissez une option: ");

                string choix = Console.ReadLine();
                Console.WriteLine();

                switch (choix)
                {
                    case "1": SaisirEtudiants(); break;
                    case "2": SaisirModifierNote(); break;
                    case "3": AfficherListeComplete(); break;
                    case "4": AfficherAdmis(); break;
                    case "5": AfficherRattrapage(); break;
                    case "6": RechercherEtudiant(); break;
                    case "7": AfficherStatistiques(); break;
                    case "8": TrierEtudiants(); break;
                    case "9": SupprimerEtudiant(); break;
                    case "10": ExporterAdmis(); break;
                    case "11": GererClasses(); break;
                    case "12":
                        continuer = false;
                        Console.WriteLine("Au revoir ! Programme terminé proprement.");
                        break;
                    default:
                        Console.WriteLine("Choix invalide. Appuyez sur une touche pour réessayer.");
                        break;
                }

                if (continuer)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour revenir au menu principal...");
                    Console.ReadKey();
                }
            }
        }

        // 1. Saisir les étudiants
        static void SaisirEtudiants()
        {
            int nombre = SaisirEntierPositif("Combien d'étudiants souhaitez-vous ajouter ? ");

            for (int i = 0; i < nombre; i++)
            {
                Console.WriteLine($"\n--- Étudiant {i + 1} / {nombre} ---");
                Console.Write("Nom: ");
                string nom = Console.ReadLine()?.Trim();

                Console.Write("Prénom: ");
                string prenom = Console.ReadLine()?.Trim();

                string matricule;
                while (true)
                {
                    Console.Write("Matricule (unique): ");
                    matricule = Console.ReadLine()?.Trim();

                    if (string.IsNullOrWhiteSpace(matricule))
                    {
                        Console.WriteLine("Le matricule ne peut pas être vide.");
                    }
                    else if (ExisteMatriculeDansToutesLesClasses(matricule))
                    {
                        Console.WriteLine("Ce matricule existe déjà dans une classe. Veuillez en entrer un unique.");
                    }
                    else
                    {
                        break;
                    }
                }

                classeActive.Etudiants.Add(new Etudiant(nom, prenom, matricule));
                Console.WriteLine("Étudiant ajouté avec succès.");
            }
        }

        // 2. Saisir / Modifier les notes
        static void SaisirModifierNote()
        {
            if (VérifierClasseVide()) return;

            Console.Write("Entrez le nom ou le matricule de l'étudiant à noter: ");
            string recherche = Console.ReadLine()?.Trim();

            Etudiant etudiant = TrouverEtudiant(recherche);

            if (etudiant == null)
            {
                Console.WriteLine("Étudiant non trouvé dans la classe active.");
                return;
            }

            Console.WriteLine($"Étudiant trouvé : {etudiant.Prenom} {etudiant.Nom} ({etudiant.Matricule})");

            double note;
            while (true)
            {
                Console.Write("Entrez la note (entre 0 et 20): ");
                if (double.TryParse(Console.ReadLine(), out note) && note >= 0 && note <= 20)
                {
                    etudiant.Note = note;
                    Console.WriteLine("Note enregistrée avec succès !");
                    break;
                }
                Console.WriteLine("Erreur: La note doit être un nombre valide compris entre 0 et 20.");
            }
        }

        // 3. Afficher la liste complète
        static void AfficherListeComplete()
        {
            if (VérifierClasseVide()) return;

            Console.WriteLine($"--- LISTE COMPLÈTE - CLASSE [{classeActive.NomClasse}] ---");
            foreach (var e in classeActive.Etudiants)
            {
                Console.WriteLine(e);
            }
        }

        // 4. Afficher les étudiants admis
        static void AfficherAdmis()
        {
            if (VérifierClasseVide()) return;

            var admis = classeActive.Etudiants.Where(e => e.Note.HasValue && e.Note.Value >= 10).ToList();

            Console.WriteLine($"--- ÉTUDIANTS ADMIS - CLASSE [{classeActive.NomClasse}] (Total: {admis.Count}) ---");
            if (admis.Count == 0)
            {
                Console.WriteLine("Aucun étudiant admis.");
            }
            else
            {
                foreach (var e in admis) Console.WriteLine(e);
            }
        }

        // 5. Afficher les étudiants à rattraper
        static void AfficherRattrapage()
        {
            if (VérifierClasseVide()) return;

            var rattrapage = classeActive.Etudiants.Where(e => e.Note.HasValue && e.Note.Value < 10).ToList();

            Console.WriteLine($"--- ÉTUDIANTS À RATTRAPER - CLASSE [{classeActive.NomClasse}] (Total: {rattrapage.Count}) ---");
            if (rattrapage.Count == 0)
            {
                Console.WriteLine("Aucun étudiant à rattraper.");
            }
            else
            {
                foreach (var e in rattrapage) Console.WriteLine(e);
            }
        }

        // 6. Rechercher un étudiant
        static void RechercherEtudiant()
        {
            if (VérifierClasseVide()) return;

            Console.Write("Entrez le nom ou le matricule recherché: ");
            string recherche = Console.ReadLine()?.Trim();

            var resultats = classeActive.Etudiants.Where(e => e.Matricule.Equals(recherche, StringComparison.OrdinalIgnoreCase) ||
                                                             e.Nom.Equals(recherche, StringComparison.OrdinalIgnoreCase)).ToList();

            if (resultats.Count == 0)
            {
                Console.WriteLine("Aucun étudiant trouvé.");
            }
            else
            {
                foreach (var e in resultats) Console.WriteLine(e);
            }
        }

        // 7. Afficher les statistiques de la classe (Moyenne + Médiane)
        static void AfficherStatistiques()
        {
            if (VérifierClasseVide()) return;

            var etudiantsNotes = classeActive.Etudiants.Where(e => e.Note.HasValue).ToList();
            if (etudiantsNotes.Count == 0)
            {
                Console.WriteLine("Aucune note n'a été saisie pour le moment.");
                return;
            }

            double moyenne = etudiantsNotes.Average(e => e.Note.Value);
            double maxNote = etudiantsNotes.Max(e => e.Note.Value);
            double minNote = etudiantsNotes.Min(e => e.Note.Value);

            // BONUS : Calcul de la médiane
            double médiane = CalculerMediane(etudiantsNotes.Select(e => e.Note.Value).ToList());

            var meilleurs = etudiantsNotes.Where(e => e.Note.Value == maxNote).Select(e => $"{e.Prenom} {e.Nom}");
            var moinsBons = etudiantsNotes.Where(e => e.Note.Value == minNote).Select(e => $"{e.Prenom} {e.Nom}");

            int admisCount = etudiantsNotes.Count(e => e.Note.Value >= 10);
            double tauxReussite = ((double)admisCount / etudiantsNotes.Count) * 100;

            Console.WriteLine($"--- STATISTIQUES - CLASSE [{classeActive.NomClasse}] ---");
            Console.WriteLine($"Moyenne générale : {moyenne:F2}/20");
            Console.WriteLine($"Médiane des notes: {médiane:F2}/20");
            Console.WriteLine($"Meilleure note    : {maxNote:F2}/20 ({string.Join(", ", meilleurs)})");
            Console.WriteLine($"Plus faible note  : {minNote:F2}/20 ({string.Join(", ", moinsBons)})");
            Console.WriteLine($"Taux de réussite  : {tauxReussite:F2}%");
        }

        // 8. Trier les étudiants
        static void TrierEtudiants()
        {
            if (VérifierClasseVide()) return;

            Console.WriteLine("1. Trier par nom (ordre alphabétique)");
            Console.WriteLine("2. Trier par note (ordre décroissant)");
            Console.Write("Faites votre choix: ");
            string choix = Console.ReadLine();

            if (choix == "1")
            {
                classeActive.Etudiants = classeActive.Etudiants.OrderBy(e => e.Nom).ThenBy(e => e.Prenom).ToList();
                Console.WriteLine("\nListe triée par nom :");
                AfficherListeComplete();
            }
            else if (choix == "2")
            {
                classeActive.Etudiants = classeActive.Etudiants.OrderByDescending(e => e.Note ?? -1).ToList();
                Console.WriteLine("\nListe triée par note décroissante :");
                AfficherListeComplete();
            }
            else
            {
                Console.WriteLine("Choix invalide.");
            }
        }

        // 9. Supprimer un étudiant
        static void SupprimerEtudiant()
        {
            if (VérifierClasseVide()) return;

            Console.Write("Entrez le nom ou le matricule de l'étudiant à supprimer: ");
            string recherche = Console.ReadLine()?.Trim();

            Etudiant etudiant = TrouverEtudiant(recherche);

            if (etudiant == null)
            {
                Console.WriteLine("Étudiant non trouvé.");
                return;
            }

            classeActive.Etudiants.Remove(etudiant);
            Console.WriteLine($"L'étudiant {etudiant.Prenom} {etudiant.Nom} a été supprimé.");
        }

        // 10. BONUS : Exporter les admis dans un fichier texte
        static void ExporterAdmis()
        {
            if (VérifierClasseVide()) return;

            var admis = classeActive.Etudiants.Where(e => e.Note.HasValue && e.Note.Value >= 10).ToList();
            string cheminFichier = $"Admis_{classeActive.NomClasse}.txt";

            using (StreamWriter writer = new StreamWriter(cheminFichier))
            {
                writer.WriteLine($"=== LISTE DES ADMIS - CLASSE {classeActive.NomClasse} ===");
                writer.WriteLine($"Date d'exportation : {DateTime.Now}");
                writer.WriteLine("----------------------------------");
                foreach (var e in admis)
                {
                    writer.WriteLine(e.ToString());
                }
            }

            Console.WriteLine($"Exportation réussie ! Fichier enregistré sous : {Path.GetFullPath(cheminFichier)}");
        }

        // 11. BONUS : Gérer et basculer entre plusieurs classes
        static void GererClasses()
        {
            Console.WriteLine("--- GESTION DES CLASSES ---");
            Console.WriteLine($"Classe courante : [{classeActive.NomClasse}]");
            Console.WriteLine("1. Changer de classe active");
            Console.WriteLine("2. Créer une nouvelle classe");
            Console.Write("Votre choix: ");
            string choix = Console.ReadLine();

            if (choix == "1")
            {
                Console.WriteLine("\nClasses disponibles :");
                for (int i = 0; i < listeClasses.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {listeClasses[i].NomClasse} ({listeClasses[i].Etudiants.Count} étudiants)");
                }
                int num = SaisirEntierPositif("Sélectionnez le numéro de la classe: ");
                if (num >= 1 && num <= listeClasses.Count)
                {
                    classeActive = listeClasses[num - 1];
                    Console.WriteLine($"Vous travaillez maintenant sur la classe [{classeActive.NomClasse}].");
                }
                else
                {
                    Console.WriteLine("Numéro de classe invalide.");
                }
            }
            else if (choix == "2")
            {
                Console.Write("Entrez le nom de la nouvelle classe (ex: 3A, L2): ");
                string nouveauNom = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(nouveauNom))
                {
                    Classe nouvelleClasse = new Classe(nouveauNom);
                    listeClasses.Add(nouvelleClasse);
                    classeActive = nouvelleClasse;
                    Console.WriteLine($"Classe [{nouveauNom}] créée et sélectionnée !");
                }
            }
        }

        // ==========================================
        // MÉTHODES UTILITAIRES ET LOGIQUE MATIÈRE
        // ==========================================

        // Calcul mathématique de la médiane
        static double CalculerMediane(List<double> notes)
        {
            var notesTriees = notes.OrderBy(n => n).ToList();
            int count = notesTriees.Count;
            if (count % 2 == 0)
            {
                return (notesTriees[(count / 2) - 1] + notesTriees[count / 2]) / 2.0;
            }
            else
            {
                return notesTriees[count / 2];
            }
        }

        static bool VérifierClasseVide()
        {
            if (classeActive.Etudiants.Count == 0)
            {
                Console.WriteLine($"La classe [{classeActive.NomClasse}] ne contient aucun étudiant pour le moment.");
                return true;
            }
            return false;
        }

        static Etudiant TrouverEtudiant(string recherche)
        {
            return classeActive.Etudiants.FirstOrDefault(e => e.Matricule.Equals(recherche, StringComparison.OrdinalIgnoreCase) ||
                                                             e.Nom.Equals(recherche, StringComparison.OrdinalIgnoreCase));
        }

        static bool ExisteMatriculeDansToutesLesClasses(string matricule)
        {
            foreach (var c in listeClasses)
            {
                if (c.Etudiants.Any(e => e.Matricule.Equals(matricule, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }
            return false;
        }

        static int SaisirEntierPositif(string message)
        {
            int val;
            while (true)
            {
                Console.Write(message);
                if (int.TryParse(Console.ReadLine(), out val) && val > 0) return val;
                Console.WriteLine("Saisie invalide. Veuillez entrer un nombre entier positif.");
            }
        }
    }
}