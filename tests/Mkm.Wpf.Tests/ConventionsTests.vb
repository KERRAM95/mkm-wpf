Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Windows
Imports System.Windows.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests

    ''' <summary>
    ''' Règles que tout contrôle public de Mkm.Wpf doit respecter. Elles s'appliquent
    ''' d'elles-mêmes à chaque contrôle ajouté : il n'y a pas de test à écrire pour cela.
    ''' </summary>
    <TestFixture>
    Public Class ConventionsTests

        Private Const GenericXaml As String = "pack://application:,,,/Mkm.Wpf;component/Themes/Generic.xaml"

        ''' <summary>Les contrôles publics et instanciables de la bibliothèque.</summary>
        Private Shared Function Controles() As IEnumerable(Of Type)
            Return Assembly.Load("Mkm.Wpf").GetExportedTypes().
                Where(Function(t) GetType(Control).IsAssignableFrom(t) AndAlso Not t.IsAbstract).
                OrderBy(Function(t) t.FullName)
        End Function

        <Test>
        Public Sub GenericXaml_SeCharge()
            Dim dictionnaire As New ResourceDictionary With {.Source = New Uri(GenericXaml)}

            Assert.That(dictionnaire.Source, [Is].EqualTo(New Uri(GenericXaml)))
        End Sub

        ''' <summary>
        ''' Sans DefaultStyleKeyProperty.OverrideMetadata dans son constructeur partagé, un
        ''' contrôle prend le style de sa classe parente au lieu du sien.
        ''' </summary>
        <Test>
        Public Sub ChaqueControle_DeclareSaCleDeStyle()
            Dim champ = GetType(FrameworkElement).GetField("DefaultStyleKeyProperty",
                BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic)
            Dim cleDeStyle = DirectCast(champ.GetValue(Nothing), DependencyProperty)

            Dim fautifs = Controles().
                Where(Function(t)
                          ' Le constructeur partagé, qui pose la métadonnée, doit avoir tourné.
                          RuntimeHelpers.RunClassConstructor(t.TypeHandle)
                          Return Not Equals(cleDeStyle.GetMetadata(t).DefaultValue, t)
                      End Function).
                Select(Function(t) t.FullName).
                ToList()

            Assert.That(fautifs, [Is].Empty, "DefaultStyleKeyProperty.OverrideMetadata manquant")
        End Sub

        ''' <summary>
        ''' Chaque contrôle a son style par défaut dans Themes/Generic.xaml (par la fusion
        ''' de son propre dictionnaire).
        ''' </summary>
        <Test>
        Public Sub ChaqueControle_AUnStyleDansGenericXaml()
            Dim dictionnaire As New ResourceDictionary With {.Source = New Uri(GenericXaml)}

            Dim sansStyle = Controles().
                Where(Function(t) TypeOf dictionnaire(t) IsNot Style).
                Select(Function(t) t.FullName).
                ToList()

            Assert.That(sansStyle, [Is].Empty, "style absent de Themes/Generic.xaml")
        End Sub

    End Class

End Namespace
