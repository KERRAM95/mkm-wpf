Imports System.Globalization
Imports System.Windows
Imports System.Windows.Markup
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf

    ''' <summary>
    ''' La configuration de mkm-wpf, posée une fois dans les ressources de l'application :
    ''' <code>
    ''' &lt;Application.Resources&gt;
    '''     &lt;ResourceDictionary&gt;
    '''         &lt;ResourceDictionary.MergedDictionaries&gt;
    '''             &lt;mkm:MkmResources Language="fr" /&gt;
    '''         &lt;/ResourceDictionary.MergedDictionaries&gt;
    '''     &lt;/ResourceDictionary&gt;
    ''' &lt;/Application.Resources&gt;
    ''' </code>
    ''' Le thème s'y ajoutera.
    ''' </summary>
    ''' <remarks>
    ''' Elle donne aussi aux éléments WPF le format régional de Windows pour les dates et les nombres :
    ''' sans cela, WPF les écrit à l'américaine (en-US), quelle que soit la région du poste.
    ''' </remarks>
    Public Class MkmResources
        Inherits ResourceDictionary

        Private Shared _formatsAppliques As Boolean

        ''' <summary>La configuration, posée dans App.xaml.</summary>
        Public Sub New()
            AppliquerFormatsRegionaux()
        End Sub

        ''' <summary>
        ''' La langue des textes, par son nom de culture (« fr », « en-GB ») : celle de
        ''' <see cref="Localizer.Current"/>. Sans elle, la langue d'affichage de Windows.
        ''' </summary>
        Public Property Language As String
            Get
                Return Localizer.Current.Language.Name
            End Get
            Set(value As String)
                Localizer.Current.Language = CultureInfo.GetCultureInfo(value)
            End Set
        End Property

        ''' <summary>
        ''' Une fois, avant le premier élément WPF : le format régional de Windows devient la langue
        ''' (<see cref="FrameworkElement.Language"/>) par défaut des éléments, que les liaisons
        ''' emploient pour écrire les dates et les nombres.
        ''' </summary>
        Private Shared Sub AppliquerFormatsRegionaux()
            If _formatsAppliques Then Return
            _formatsAppliques = True

            Dim region = New FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag))
            Try
                FrameworkElement.LanguageProperty.OverrideMetadata(GetType(FrameworkElement), region)
            Catch ex As InvalidOperationException
                ' Un élément WPF existait déjà (concepteur XAML de Visual Studio) : WPF garde en-US.
            Catch ex As ArgumentException
                ' La métadonnée était déjà remplacée par l'application : elle a le dernier mot.
            End Try
        End Sub

    End Class

End Namespace
