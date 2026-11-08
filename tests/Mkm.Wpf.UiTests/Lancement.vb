Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>Une vitrine pour tous les scénarios : lancée avant le premier, fermée après le dernier.</summary>
    <SetUpFixture>
    Public Class Lancement

        <OneTimeSetUp>
        Public Sub Lancer()
            Vitrine.Lancer()
        End Sub

        <OneTimeTearDown>
        Public Sub Fermer()
            Vitrine.Arreter()
            Vitrine.Automation.Dispose()
        End Sub

    End Class

End Namespace
