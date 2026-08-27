using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

    class Program
    {
        static List<Classe> listeClasses = new List<Classe>();
        static Classe classeActive = null;

        static void Main(string[] args)
        {
            Console.Clear();
            AfficherEnteteDecoratif("INITIALISATION DU SYSTÈME");
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
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("===================================================================");
                Console.WriteLine($"         GESTION DE CLASSE  |  Classe active : [{classeActive.NomClasse}]");
                Console.WriteLine("===================================================================");
                Console.ResetColor();
                Console.WriteLine(" 1. Saisir les étudiants");
                Console.WriteLine(" 2. Saisir / Modifier les notes");
                Console.WriteLine(" 3. Afficher la liste complète");
                Console.WriteLine(" 4. Afficher les étudiants admis");
                Console.WriteLine(" 5. Afficher les étudiants à rattraper");
                Console.WriteLine(" 6. Rechercher un étudiant");
                Console.WriteLine(" 7. Afficher les statistiques (Moyenne + Médiane)");
                Console.WriteLine(" 8. Trier les étudiants (par nom ou par note)");
                Console.WriteLine(" 9. Supprimer un étudiant");
                Console.WriteLine("10. Exporter la liste des admis (Fichier .txt)");
                Console.WriteLine("11. Gérer / Changer de classe");
                Console.WriteLine("12. Quitter");
                Console.WriteLine("===================================================================");
                Console.Write("Choisissez une option (1-12): ");

                string choix = Console.ReadLine();

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
                        Console.Clear();
                        AfficherEnteteDecoratif("FIN DU PROGRAMME");
                        Console.WriteLine("Au revoir ! Le programme s'est terminé proprement.\n");
                        continuer = false;
                        break;
                    default:
                        Console.WriteLine("\n[!] Choix invalide.");
                        PauseRetourMenu();
                        break;
                }
            }
        }

        // --- DECORATION ET NAVIGATION ---

        static void AfficherEnteteDecoratif(string titre)
        {
            // Remplacé ConsoleColor.Yellow par ConsoleColor.Magenta (Rose)
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("===================================================================");
            Console.WriteLine($"                     {titre.ToUpper()}");
            Console.WriteLine("===================================================================");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void PauseRetourMenu()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n-------------------------------------------------------------------");
            Console.WriteLine(" Appuyez sur n'importe quelle touche pour revenir au menu... ");
            Console.WriteLine("-------------------------------------------------------------------");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        // --- FONCTIONNALITÉS ---

        static void SaisirEtudiants()
        {
            Console.Clear();
            AfficherEnteteDecoratif("1. SAISIR LES ÉTUDIANTS");

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
                        Console.WriteLine("[!] Le matricule ne peut pas être vide.");
                    }
                    else if (ExisteMatriculeDansToutesLesClasses(matricule))
                    {
                        Console.WriteLine("[!] Ce matricule existe déjà dans le système.");
                    }
                    else
                    {
                        break;
                    }
                }

                classeActive.Etudiants.Add(new Etudiant(nom, prenom, matricule));
                Console.WriteLine($"[✓] Étudiant {prenom} {nom} ajouté avec succès.");
            }

            PauseRetourMenu();
        }

        static void SaisirModifierNote()
        {
            Console.Clear();
            AfficherEnteteDecoratif("2. SAISIR / MODIFIER LES NOTES");

            if (VérifierClasseVide()) { PauseRetourMenu(); return; }

            Console.Write("Entrez le nom ou le matricule de l'étudiant à noter: ");
            string recherche = Console.ReadLine()?.Trim();

            Etudiant etudiant = TrouverEtudiant(recherche);

            if (etudiant == null)
            {
                Console.WriteLine("[!] Étudiant non trouvé dans la classe active.");
                PauseRetourMenu();
                return;
            }

            Console.WriteLine($"\nÉtudiant trouvé : {etudiant.Prenom} {etudiant.Nom} ({etudiant.Matricule})");

            double note;
            while (true)
            {
                Console.Write("Entrez la note (entre 0 et 20): ");
                if (double.TryParse(Console.ReadLine(), out note) && note >= 0 && note <= 20)
                {
                    etudiant.Note = note;
                    Console.WriteLine("[✓] Note enregistrée avec succès !");
                    break;
                }
                Console.WriteLine("[!] Erreur: La note doit être un nombre valide entre 0 et 20.");
            }

            PauseRetourMenu();
        }

        static void AfficherListeComplete()
        {
            Console.Clear();
            AfficherEnteteDecoratif($"3. LISTE COMPLÈTE - CLASSE [{classeActive.NomClasse}]");

            if (VérifierClasseVide()) { PauseRetourMenu(); return; }

            foreach (var e in classeActive.Etudiants)
            {
                Console.WriteLine(e);
            }

            PauseRetourMenu();
        }

        static void AfficherAdmis()
        {
            Console.Clear();
            AfficherEnteteDecoratif($"4. ÉTUDIANTS ADMIS - CLASSE [{classeActive.NomClasse}]");

            if (VérifierClasseVide()) { PauseRetourMenu(); return; }

            var admis = classeActive.Etudiants.Where(e => e.Note.HasValue && e.Note.Value >= 10).ToList();

            Console.WriteLine($"Nombre total d'admis : {admis.Count}\n");
            if (admis.Count == 0)
            {
                Console.WriteLine("Aucun étudiant n'est admis pour le moment.");
            }
            else
            {
                foreach (var e in admis) Console.WriteLine(e);
            }

            PauseRetourMenu();
        }

        static void AfficherRattrapage()
        {
            Console.Clear();
            AfficherEnteteDecoratif($"5. ÉTUDIANTS À RATTRAPER - CLASSE [{classeActive.NomClasse}]");

            if (VérifierClasseVide()) { PauseRetourMenu(); return; }

            var rattrapage = classeActive.Etudiants.Where(e => e.Note.HasValue && e.Note.Value < 10).ToList();

            Console.WriteLine($"Nombre total à rattraper : {rattrapage.Count}\n");
            if (rattrapage.Count == 0)
            {
                Console.WriteLine("Aucun étudiant en rattrapage.");
            }
            else
            {
                foreach (var e in rattrapage) Console.WriteLine(e);
            }

            PauseRetourMenu();
        }

        static void RechercherEtudiant()
        {
            Console.Clear();
            AfficherEnteteDecoratif("6. RECHERCHER UN ÉTUDIANT");

            if (VérifierClasseVide()) { PauseRetourMenu(); return; }

            Console.Write("Entrez le nom ou le matricule recherché: ");
            string recherche = Console.ReadLine()?.Trim();

            var resultats = classeActive.Etudiants.Where(e => e.Matricule.Equals(recherche, StringComparison.OrdinalIgnoreCase) ||
                                                             e.Nom.Equals(recherche, StringComparison.OrdinalIgnoreCase)).ToList();

            Console.WriteLine();
            if (resultats.Count == 0)
            {
                Console.WriteLine("[!] Aucun étudiant trouvé.");
            }
            else
            {
                Console.WriteLine($"Résultat(s) trouvé(s) ({resultats.Count}) :");
                foreach (var e in resultats) Console.WriteLine(e);
            }

            PauseRetourMenu();
        }

        static void AfficherStatistiques()
        {
            Console.Clear();
            AfficherEnteteDecoratif($"7. STATISTIQUES - CLASSE [{classeActive.NomClasse}]");

            if (VérifierClasseVide()) { PauseRetourMenu(); return; }

            var etudiantsNotes = classeActive.Etudiants.Where(e => e.Note.HasValue).ToList();
            if (etudiantsNotes.Count == 0)
            {
                Console.WriteLine("Aucune note n'a été saisie pour le moment.");
                PauseRetourMenu();
                return;
            }

            double moyenne = etudiantsNotes.Average(e => e.Note.Value);
            double maxNote = etudiantsNotes.Max(e => e.Note.Value);
            double minNote = etudiantsNotes.Min(e => e.Note.Value);
            double mediane = CalculerMediane(etudiantsNotes.Select(e => e.Note.Value).ToList());

            var meilleurs = etudiantsNotes.Where(e => e.Note.Value == maxNote).Select(e => $"{e.Prenom} {e.Nom}");
            var moinsBons = etudiantsNotes.Where(e => e.Note.Value == minNote).Select(e => $"{e.Prenom} {e.Nom}");

            int admisCount = etudiantsNotes.Count(e => e.Note.Value >= 10);
            double tauxReussite = ((double)admisCount / etudiantsNotes.Count) * 100;

            Console.WriteLine($"Moyenne générale  : {moyenne:F2}/20");
            Console.WriteLine($"Médiane des notes : {mediane:F2}/20");
            Console.WriteLine($"Meilleure note     : {maxNote:F2}/20 par ({string.Join(", ", meilleurs)})");
            Console.WriteLine($"Plus faible note   : {minNote:F2}/20 par ({string.Join(", ", moinsBons)})");
            Console.WriteLine($"Taux de réussite   : {tauxReussite:F2}%");

            PauseRetourMenu();
        }

        static void TrierEtudiants()
        {
            Console.Clear();
            AfficherEnteteDecoratif("8. TRIER LES ÉTUDIANTS");

            if (VérifierClasseVide()) { PauseRetourMenu(); return; }

            Console.WriteLine("1. Trier par nom (ordre alphabétique)");
            Console.WriteLine("2. Trier par note (ordre décroissant)");
            Console.Write("\nFaites votre choix: ");
            string choix = Console.ReadLine();

            Console.WriteLine();
            if (choix == "1")
            {
                classeActive.Etudiants = classeActive.Etudiants.OrderBy(e => e.Nom).ThenBy(e => e.Prenom).ToList();
                Console.WriteLine("[✓] Tri par nom effectué :");
                foreach (var e in classeActive.Etudiants) Console.WriteLine(e);
            }
            else if (choix == "2")
            {
                classeActive.Etudiants = classeActive.Etudiants.OrderByDescending(e => e.Note ?? -1).ToList();
                Console.WriteLine("[✓] Tri par note décroissante effectué :");
                foreach (var e in classeActive.Etudiants) Console.WriteLine(e);
            }
            else
            {
                Console.WriteLine("[!] Choix invalide.");
            }

            PauseRetourMenu();
        }

        static void SupprimerEtudiant()
        {
            Console.Clear();
            AfficherEnteteDecoratif("9. SUPPRIMER UN ÉTUDIANT");

            if (VérifierClasseVide()) { PauseRetourMenu(); return; }

            Console.Write("Entrez le nom ou le matricule de l'étudiant à supprimer: ");
            string recherche = Console.ReadLine()?.Trim();

            Etudiant etudiant = TrouverEtudiant(recherche);

            if (etudiant == null)
            {
                Console.WriteLine("[!] Étudiant non trouvé.");
                PauseRetourMenu();
                return;
            }

            classeActive.Etudiants.Remove(etudiant);
            Console.WriteLine($"[✓] L'étudiant {etudiant.Prenom} {etudiant.Nom} ({etudiant.Matricule}) a été supprimé.");

            PauseRetourMenu();
        }

        static void ExporterAdmis()
        {
            Console.Clear();
            AfficherEnteteDecoratif("10. EXPORTER LES ADMIS");

            if (VérifierClasseVide()) { PauseRetourMenu(); return; }

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

            Console.WriteLine($"[✓] Fichier généré avec succès !");
            Console.WriteLine($"Emplacement : {Path.GetFullPath(cheminFichier)}");

            PauseRetourMenu();
        }

        static void GererClasses()
        {
            Console.Clear();
            AfficherEnteteDecoratif("11. GÉRER / CHANGER DE CLASSE");

            Console.WriteLine($"Classe actuelle : [{classeActive.NomClasse}]\n");
            Console.WriteLine("1. Changer de classe active");
            Console.WriteLine("2. Créer une nouvelle classe");
            Console.Write("\nVotre choix: ");
            string choix = Console.ReadLine();

            if (choix == "1")
            {
                Console.WriteLine("\nClasses disponibles :");
                for (int i = 0; i < listeClasses.Count; i++)
                {
                    Console.WriteLine($" {i + 1}. {listeClasses[i].NomClasse} ({listeClasses[i].Etudiants.Count} étudiants)");
                }
                int num = SaisirEntierPositif("\nSélectionnez le numéro de la classe: ");
                if (num >= 1 && num <= listeClasses.Count)
                {
                    classeActive = listeClasses[num - 1];
                    Console.WriteLine($"[✓] Vous travaillez maintenant sur la classe [{classeActive.NomClasse}].");
                }
                else
                {
                    Console.WriteLine("[!] Numéro invalide.");
                }
            }
            else if (choix == "2")
            {
                Console.Write("\nEntrez le nom de la nouvelle classe: ");
                string nouveauNom = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(nouveauNom))
                {
                    Classe nouvelleClasse = new Classe(nouveauNom);
                    listeClasses.Add(nouvelleClasse);
                    classeActive = nouvelleClasse;
                    Console.WriteLine($"[✓] Classe [{nouveauNom}] créée et sélectionnée !");
                }
            }

            PauseRetourMenu();
        }

        // --- OUTILS COMPLÉMENTAIRES ---

        static double CalculerMediane(List<double> notes)
        {
            var notesTriees = notes.OrderBy(n => n).ToList();
            int count = notesTriees.Count;
            if (count % 2 == 0)
            {
                return (notesTriees[(count / 2) - 1] + notesTriees[count / 2]) / 2.0;
            }
            return notesTriees[count / 2];
        }

        static bool VérifierClasseVide()
        {
            if (classeActive.Etudiants.Count == 0)
            {
                Console.WriteLine($"La classe [{classeActive.NomClasse}] ne contient aucun étudiant.");
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
                Console.WriteLine("[!] Saisie invalide. Veuillez entrer un nombre entier positif.");
            }
        }
    }
}