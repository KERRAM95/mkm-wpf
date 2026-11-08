Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Les boutons Suivant et Précédent des étapes.</summary>
    Friend NotInheritable Class StepperBoutons

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            exemple.AddHandler(ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf Bouton_Click))
        End Sub

#Region "Exemple"
        ' Le clic d'un bouton d'une étape remonte au Stepper : Next() ou Previous().
        Private Shared Sub Bouton_Click(sender As Object, e As RoutedEventArgs)
            Dim bouton = TryCast(e.OriginalSource, FrameworkElement)
            Dim etapes = Trouver(bouton)
            If etapes Is Nothing Then Return
            Select Case CStr(bouton.Tag)
                Case "suivant" : etapes.Next()
                Case "precedent" : etapes.Previous()
            End Select
        End Sub
#End Region

        ' Le Stepper de la page qui contient le bouton.
        Private Shared Function Trouver(element As DependencyObject) As Stepper
            While element IsNot Nothing AndAlso Not TypeOf element Is Stepper
                element = If(LogicalTreeHelper.GetParent(element), Media.VisualTreeHelper.GetParent(element))
            End While
            Return TryCast(element, Stepper)
        End Function

    End Class

End Namespace
