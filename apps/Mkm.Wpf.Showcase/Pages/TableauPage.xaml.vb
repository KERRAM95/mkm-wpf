Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Threading
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Showcase

    ''' <summary>Une variable d'environnement de l'exemple.</summary>
    Public Class VariableExemple
        Public Property Nom As String
        Public Property Valeur As String
        Public Property Portee As String
        Public Property Modifiee As Date
        Public Property Active As Boolean
    End Class

    ''' <summary>La page du tableau de données : des variables d'environnement d'exemple, un chargement de 2 s.</summary>
    Partial Public Class TableauPage

        Public Sub New()
            InitializeComponent()
            Dim jour As New Date(2026, 11, 7)
            Dim liste As New ObservableCollection(Of VariableExemple) From {
                New VariableExemple With {.Nom = "HSCART_TENANT", .Valeur = "R1132100000001", .Portee = "Utilisateur", .Modifiee = jour, .Active = True},
                New VariableExemple With {.Nom = "CATIA_LANGUAGE", .Valeur = "fr", .Portee = "Système", .Modifiee = jour.AddDays(-3), .Active = True},
                New VariableExemple With {.Nom = "CATDLLPath", .Valeur = "C:\Program Files\Dassault Systemes\B32\win_b64\code\bin", .Portee = "Système", .Modifiee = jour.AddDays(-30), .Active = True},
                New VariableExemple With {.Nom = "CATReferenceSettingPath", .Valeur = "\\serveur\catia\reference", .Portee = "Système", .Modifiee = jour.AddDays(-12), .Active = False},
                New VariableExemple With {.Nom = "CATUserSettingPath", .Valeur = "%APPDATA%\DassaultSystemes\CATSettings", .Portee = "Utilisateur", .Modifiee = jour.AddDays(-1), .Active = True},
                New VariableExemple With {.Nom = "CATTemp", .Valeur = "%TEMP%\CATTemp", .Portee = "Utilisateur", .Modifiee = jour.AddDays(-60), .Active = True},
                New VariableExemple With {.Nom = "HSCART_LOG_LEVEL", .Valeur = "Info", .Portee = "Utilisateur", .Modifiee = jour.AddDays(-2), .Active = True},
                New VariableExemple With {.Nom = "HSCART_3DX_URL", .Valeur = "https://3dexperience.exemple.fr", .Portee = "Système", .Modifiee = jour.AddDays(-5), .Active = True}
            }
            Variables.ItemsSource = liste
            Variables.SelectedIndex = 0
            Zebree.ItemsSource = liste
            Nombre.Text = Localizer.Current.T("showcase.table.count", liste.Count)
        End Sub

        Private Sub Charger_Click(sender As Object, e As RoutedEventArgs)
            Variables.Loading = True
            Dim minuterie As New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(2)}
            AddHandler minuterie.Tick, Sub()
                                           minuterie.Stop()
                                           Variables.Loading = False
                                       End Sub
            minuterie.Start()
        End Sub

    End Class

End Namespace
