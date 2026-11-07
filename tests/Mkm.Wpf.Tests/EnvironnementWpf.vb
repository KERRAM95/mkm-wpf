Imports System.Windows
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests

    ''' <summary>
    ''' Prépare WPF une fois pour tous les tests de l'assembly.
    ''' </summary>
    <SetUpFixture>
    Public NotInheritable Class EnvironnementWpf

        Private Shared _application As Application

        ''' <summary>
        ''' Crée l'application WPF : sans elle, le schéma pack://application n'existe pas
        ''' et aucun dictionnaire de ressources de la bibliothèque ne se charge.
        ''' </summary>
        <OneTimeSetUp>
        Public Sub Preparer()
            If Application.Current Is Nothing Then
                _application = New Application With {.ShutdownMode = ShutdownMode.OnExplicitShutdown}
            End If
        End Sub

    End Class

End Namespace
