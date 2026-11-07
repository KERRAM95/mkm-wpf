Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le choix d'une date de mkm (<c>DatePicker</c>) : le champ de 24 px et, accolé, le bouton du calendrier ; le
    ''' calendrier s'ouvre dessous, un mois à la fois, les jours dans des cercles de 24 px, le jour choisi de la
    ''' couleur principale, aujourd'hui en gris. La date s'écrit au format régional de Windows.
    ''' <code>&lt;mkm:DatePicker SelectedDate="{Binding DateDuPlan}" Placeholder="jj/mm/aaaa" /&gt;</code>
    ''' </summary>
    ''' <remarks>
    ''' C'est le <see cref="System.Windows.Controls.DatePicker"/> de WPF : la saisie au clavier, Alt+Bas qui ouvre
    ''' le calendrier, les flèches et Pg.Préc./Pg.Suiv. qui s'y promènent, <c>DisplayDateStart</c>,
    ''' <c>BlackoutDates</c>, l'accessibilité.
    ''' </remarks>
    Public Class DatePicker
        Inherits System.Windows.Controls.DatePicker

        ''' <summary>Le texte d'aide, montré tant que le champ est vide (aucun par défaut).</summary>
        Public Shared ReadOnly PlaceholderProperty As DependencyProperty = InputText.PlaceholderProperty.AddOwner(GetType(DatePicker))

        ''' <summary>Vrai si la date est refusée : le champ se borde de la couleur d'erreur.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = InputText.InvalidProperty.AddOwner(GetType(DatePicker))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(DatePicker), New FrameworkPropertyMetadata(GetType(DatePicker)))
        End Sub

        ''' <summary>Le texte d'aide, montré tant que le champ est vide.</summary>
        Public Property Placeholder As String
            Get
                Return CStr(GetValue(PlaceholderProperty))
            End Get
            Set(value As String)
                SetValue(PlaceholderProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si la date est refusée.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

    End Class

End Namespace
