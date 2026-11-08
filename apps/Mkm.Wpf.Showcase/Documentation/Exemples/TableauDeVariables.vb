Imports System
Imports System.Collections.ObjectModel
Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Une variable d'environnement : une ligne des tableaux d'exemple.</summary>
    Public NotInheritable Class VariableDEnvironnement
        Public Property Nom As String
        Public Property Valeur As String
        Public Property Portee As String
        Public Property Modifiee As Date
        Public Property Active As Boolean
    End Class

    ''' <summary>Les tableaux de l'exemple reçoivent des variables d'environnement ; un tableau marqué « vide » n'en reçoit pas.</summary>
    Friend NotInheritable Class TableauDeVariables

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            For Each tableau In Tous(Of DataTable)(exemple)
                If Not Equals(tableau.Tag, "vide") Then Remplir(tableau)
            Next
        End Sub

#Region "Exemple"
        ' Les lignes du tableau : une collection d'objets, une propriété par colonne (Binding="{Binding Nom}").
        Friend Shared Sub Remplir(tableau As DataTable)
            Dim jour As New Date(2026, 11, 7)
            tableau.ItemsSource = New ObservableCollection(Of VariableDEnvironnement) From {
                New VariableDEnvironnement With {.Nom = "HSCART_TENANT", .Valeur = "R1132100000001", .Portee = "Utilisateur", .Modifiee = jour, .Active = True},
                New VariableDEnvironnement With {.Nom = "CATIA_LANGUAGE", .Valeur = "fr", .Portee = "Système", .Modifiee = jour.AddDays(-3), .Active = True},
                New VariableDEnvironnement With {.Nom = "CATDLLPath", .Valeur = "C:\Program Files\Dassault Systemes\B32\win_b64\code\bin", .Portee = "Système", .Modifiee = jour.AddDays(-30), .Active = True},
                New VariableDEnvironnement With {.Nom = "CATReferenceSettingPath", .Valeur = "\\serveur\catia\reference", .Portee = "Système", .Modifiee = jour.AddDays(-12), .Active = False},
                New VariableDEnvironnement With {.Nom = "CATUserSettingPath", .Valeur = "%APPDATA%\DassaultSystemes\CATSettings", .Portee = "Utilisateur", .Modifiee = jour.AddDays(-1), .Active = True},
                New VariableDEnvironnement With {.Nom = "CATTemp", .Valeur = "%TEMP%\CATTemp", .Portee = "Utilisateur", .Modifiee = jour.AddDays(-60), .Active = True},
                New VariableDEnvironnement With {.Nom = "HSCART_LOG_LEVEL", .Valeur = "Info", .Portee = "Utilisateur", .Modifiee = jour.AddDays(-2), .Active = True},
                New VariableDEnvironnement With {.Nom = "HSCART_3DX_URL", .Valeur = "https://3dexperience.exemple.fr", .Portee = "Système", .Modifiee = jour.AddDays(-5), .Active = True}
            }
        End Sub
#End Region

    End Class

End Namespace
