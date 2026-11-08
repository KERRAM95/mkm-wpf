Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Trouver les éléments d'un exemple : ceux d'un type, dans son arbre logique.</summary>
    Friend Module Descendance

        ''' <summary>Les éléments de type <typeparamref name="T"/> sous <paramref name="racine"/>, elle comprise.</summary>
        Friend Function Tous(Of T As DependencyObject)(racine As DependencyObject) As List(Of T)
            Dim trouves As New List(Of T)()
            Rassembler(racine, trouves)
            Return trouves
        End Function

        Private Sub Rassembler(Of T As DependencyObject)(element As DependencyObject, trouves As List(Of T))
            Dim cherche = TryCast(element, T)
            If cherche IsNot Nothing Then trouves.Add(cherche)
            For Each enfant In LogicalTreeHelper.GetChildren(element).OfType(Of DependencyObject)()
                Rassembler(enfant, trouves)
            Next
        End Sub

    End Module

End Namespace
