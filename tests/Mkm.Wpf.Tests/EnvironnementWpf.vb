Imports System.Windows
Imports Mkm.Wpf
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests

    ''' <summary>
    ''' Prépare WPF une fois pour tous les tests de l'assembly.
    ''' </summary>
    <SetUpFixture>
    Public NotInheritable Class EnvironnementWpf

        Private Shared _application As Application
        Private Shared _ressources As MkmResources

        ''' <summary>
        ''' Pose la configuration de mkm-wpf, puis crée l'application WPF : sans elle, le schéma pack://application n'existe pas
        ''' et aucun dictionnaire de ressources de la bibliothèque ne se charge.
        ''' </summary>
        <OneTimeSetUp>
        Public Sub Preparer()
            ' La configuration d'abord, comme App.xaml : avant tout élément WPF.
            _ressources = New MkmResources()

            If Application.Current Is Nothing Then
                _application = New Application With {.ShutdownMode = ShutdownMode.OnExplicitShutdown}
            End If
            ' Les ressources du thème, comme App.xaml les pose : les gabarits y lisent leurs jetons.
            Application.Current.Resources.MergedDictionaries.Add(_ressources)
        End Sub

    End Class

End Namespace
