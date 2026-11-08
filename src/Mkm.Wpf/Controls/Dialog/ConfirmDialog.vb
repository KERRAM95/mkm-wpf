Imports System.Globalization
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports Mkm.Wpf.Localization
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La demande de confirmation de mkm (<c>ConfirmDialog</c>) : un <see cref="Dialog"/> qui pose une question,
    ''' une icône de 24 px devant elle, 12 px entre elles, et deux boutons, Non puis Oui (les textes de la
    ''' bibliothèque, <c>locale.reject</c> et <c>locale.accept</c>). <see cref="Confirm"/> l'ouvre et rend la
    ''' réponse.
    ''' <code>If ConfirmDialog.Confirm(Me, "Mettre à jour le cartouche ?", "Mise à jour", Icons.Warning) Then …</code>
    ''' </summary>
    ''' <remarks>
    ''' Comme chez mkm, les deux boutons sont principaux par défaut ; <see cref="RejectSeverity"/> les distingue.
    ''' Entrée accepte, Échap refuse ; le focus est sur Oui (<c>defaultFocus</c> de PrimeVue).
    ''' </remarks>
    Public Class ConfirmDialog
        Inherits Dialog

        ''' <summary>La question.</summary>
        Public Shared ReadOnly MessageProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Message), GetType(Object), GetType(ConfirmDialog), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'icône devant la question (aucune par défaut ; <c>Icon</c> est celle de la fenêtre).</summary>
        Public Shared ReadOnly MessageIconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(MessageIcon), GetType(Object), GetType(ConfirmDialog), New FrameworkPropertyMetadata(Nothing, AddressOf OnMessageIconChanged))

        ''' <summary>Le texte du bouton qui accepte (par défaut « Oui », <c>locale.accept</c>).</summary>
        Public Shared ReadOnly AcceptLabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(AcceptLabel), GetType(String), GetType(ConfirmDialog), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le texte du bouton qui refuse (par défaut « Non », <c>locale.reject</c>).</summary>
        Public Shared ReadOnly RejectLabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(RejectLabel), GetType(String), GetType(ConfirmDialog), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La gravité du bouton qui accepte (principal par défaut).</summary>
        Public Shared ReadOnly AcceptSeverityProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(AcceptSeverity), GetType(ButtonSeverity), GetType(ConfirmDialog), New FrameworkPropertyMetadata(ButtonSeverity.Primary))

        ''' <summary>La gravité du bouton qui refuse (principal par défaut, comme chez mkm).</summary>
        Public Shared ReadOnly RejectSeverityProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(RejectSeverity), GetType(ButtonSeverity), GetType(ConfirmDialog), New FrameworkPropertyMetadata(ButtonSeverity.Primary))

        Private ReadOnly _accepter As New Button()
        Private ReadOnly _refuser As New Button()
        Private ReadOnly _icone As New Mkm.Wpf.Controls.Icon With {.Visibility = Visibility.Collapsed, .VerticalAlignment = VerticalAlignment.Center}

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ConfirmDialog), New FrameworkPropertyMetadata(GetType(ConfirmDialog)))
        End Sub

        ''' <summary>Une demande sans question encore ; Oui la ferme sur <c>DialogResult = True</c>.</summary>
        Public Sub New()
            _refuser.IsCancel = True
            _accepter.IsDefault = True
            Lier(_accepter, AcceptSeverityProperty, AcceptLabelProperty, "locale.accept")
            Lier(_refuser, RejectSeverityProperty, RejectLabelProperty, "locale.reject")
            AddHandler _accepter.Click, Sub() Repondre(True)
            AddHandler _refuser.Click, Sub() Repondre(False)

            Dim pied As New StackPanel With {.Orientation = Orientation.Horizontal}
            pied.Children.Add(_refuser)
            pied.Children.Add(_accepter)
            Footer = pied

            ' Le contenu de mkm : l'icône de 24 px, 12 px, la question, centrées en hauteur.
            _icone.SetResourceReference(WidthProperty, ThemeResources.Prefix & "confirmdialog.icon.size")
            _icone.SetResourceReference(HeightProperty, ThemeResources.Prefix & "confirmdialog.icon.size")
            _icone.SetResourceReference(Mkm.Wpf.Controls.Icon.ForegroundProperty, ThemeResources.Prefix & "confirmdialog.icon.color")
            _icone.SetResourceReference(MarginProperty, ThemeResources.Prefix & "confirmdialog.icon.margin")
            _icone.SetBinding(Mkm.Wpf.Controls.Icon.SourceProperty, New Binding(NameOf(MessageIcon)) With {.Source = Me})
            Dim question As New ContentPresenter With {.VerticalAlignment = VerticalAlignment.Center}
            ' Une fenêtre à la taille de son contenu se mesure sur une largeur infinie : sans largeur maximale, une longue
            ' question ne passerait jamais à la ligne.
            question.SetResourceReference(MaxWidthProperty, ThemeResources.Prefix & "confirmdialog.message.max.width")
            question.SetBinding(ContentPresenter.ContentProperty, New Binding(NameOf(Message)) With {.Source = Me})
            Dim enLignes As New Style(GetType(TextBlock))
            enLignes.Setters.Add(New Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap))
            question.Resources.Add(GetType(TextBlock), enLignes)
            Dim contenu As New DockPanel()
            DockPanel.SetDock(_icone, System.Windows.Controls.Dock.Left)
            contenu.Children.Add(_icone)
            contenu.Children.Add(question)
            Content = contenu

            AddHandler Loaded, Sub() _accepter.Focus()
        End Sub

        ''' <summary>La question.</summary>
        Public Property Message As Object
            Get
                Return GetValue(MessageProperty)
            End Get
            Set(value As Object)
                SetValue(MessageProperty, value)
            End Set
        End Property

        ''' <summary>L'icône devant la question (aucune par défaut).</summary>
        Public Property MessageIcon As Object
            Get
                Return GetValue(MessageIconProperty)
            End Get
            Set(value As Object)
                SetValue(MessageIconProperty, value)
            End Set
        End Property

        ''' <summary>Le texte du bouton qui accepte (par défaut « Oui »).</summary>
        Public Property AcceptLabel As String
            Get
                Return CStr(GetValue(AcceptLabelProperty))
            End Get
            Set(value As String)
                SetValue(AcceptLabelProperty, value)
            End Set
        End Property

        ''' <summary>Le texte du bouton qui refuse (par défaut « Non »).</summary>
        Public Property RejectLabel As String
            Get
                Return CStr(GetValue(RejectLabelProperty))
            End Get
            Set(value As String)
                SetValue(RejectLabelProperty, value)
            End Set
        End Property

        ''' <summary>La gravité du bouton qui accepte (principal par défaut).</summary>
        Public Property AcceptSeverity As ButtonSeverity
            Get
                Return DirectCast(GetValue(AcceptSeverityProperty), ButtonSeverity)
            End Get
            Set(value As ButtonSeverity)
                SetValue(AcceptSeverityProperty, value)
            End Set
        End Property

        ''' <summary>La gravité du bouton qui refuse (principal par défaut).</summary>
        Public Property RejectSeverity As ButtonSeverity
            Get
                Return DirectCast(GetValue(RejectSeverityProperty), ButtonSeverity)
            End Get
            Set(value As ButtonSeverity)
                SetValue(RejectSeverityProperty, value)
            End Set
        End Property

        ''' <summary>Le bouton qui accepte, pour le régler davantage (icône…).</summary>
        Public ReadOnly Property AcceptButton As Button
            Get
                Return _accepter
            End Get
        End Property

        ''' <summary>Le bouton qui refuse.</summary>
        Public ReadOnly Property RejectButton As Button
            Get
                Return _refuser
            End Get
        End Property

        ''' <summary>
        ''' Ouvre une demande modale sur <paramref name="owner"/> et rend vrai si l'utilisateur accepte (Oui ou
        ''' Entrée), faux s'il refuse, ferme la fenêtre ou appuie sur Échap.
        ''' </summary>
        Public Shared Function Confirm(owner As Window, message As Object, Optional header As String = Nothing,
                                       Optional icon As Object = Nothing) As Boolean
            Dim demande As New ConfirmDialog With {.Message = message, .Title = If(header, String.Empty), .MessageIcon = icon}
            If owner IsNot Nothing Then demande.Owner = owner
            Return demande.ShowDialog().GetValueOrDefault()
        End Function

        Private Shared Sub OnMessageIconChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, ConfirmDialog)._icone.Visibility = If(e.NewValue Is Nothing, Visibility.Collapsed, Visibility.Visible)
        End Sub

        Private Sub Repondre(oui As Boolean)
            ' Ouverte par ShowDialog, la réponse ferme la fenêtre ; ouverte par Show, on la ferme.
            Try
                DialogResult = oui
            Catch ex As InvalidOperationException
                Close()
            End Try
        End Sub

        ' Le texte du bouton suit la propriété, sinon le texte de la bibliothèque dans la langue choisie.
        Private Sub Lier(bouton As Button, gravite As DependencyProperty, libelle As DependencyProperty, cle As String)
            bouton.SetBinding(Button.SeverityProperty, New Binding(gravite.Name) With {.Source = Me})
            Dim texte As New MultiBinding With {.Converter = New PremierNonVide()}
            texte.Bindings.Add(New Binding(libelle.Name) With {.Source = Me})
            texte.Bindings.Add(New Binding($"[{cle}]") With {.Source = Localizer.Current})
            bouton.SetBinding(ContentControl.ContentProperty, texte)
        End Sub

        ' La première valeur qui est un texte non vide.
        Private NotInheritable Class PremierNonVide
            Implements IMultiValueConverter

            Public Function Convert(values As Object(), targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
                Return values.FirstOrDefault(Function(v) TypeOf v Is String AndAlso CStr(v).Length > 0)
            End Function

            Public Function ConvertBack(value As Object, targetTypes As Type(), parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
                Throw New NotSupportedException()
            End Function
        End Class

    End Class

End Namespace
