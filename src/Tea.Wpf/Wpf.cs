using System;
using System.Windows;
using System.Windows.Controls;
using ImTools;

namespace Tea.Wpf
{
    public class WpfUI : INativeUI
    {
        /// <summary>Initializes the UI bound to provided content control.</summary>
        public static INativeUI Init(ContentControl root) => new WpfUI(root);

        private readonly ContentControl _rootControl;
        private WpfUI(ContentControl rootControl) => _rootControl = rootControl;

        /// <summary>Applies the updates.</summary>
        public void ApplyDiffs(ImList<UIDiff> diffs) =>
            diffs.Do(update => _rootControl.Dispatcher.Invoke(() => Apply(update, _rootControl)));

        private static void Apply(UIDiff diff, ContentControl root)
        {
            //var path = diff.Path;
            var (pos, tail, isEmpty) = diff.Path;
            switch (diff)
            {
                case UIDiff.Insert insert:
                    if (isEmpty)
                        root.Content = CreateUI(insert.UI);
                    else
                        Locate(tail, root).Children.Insert(pos, CreateUI(insert.UI));
                    break;

                case UIDiff.Update update:
                    var elem = isEmpty ? (UIElement)root.Content : Locate(tail, root).Children[pos];
                    Update(update.UI, elem);
                    break;

                case UIDiff.Replace replace:
                    if (isEmpty)
                        root.Content = CreateUI(replace.UI);
                    else
                    {
                        var children = Locate(tail, root).Children;
                        children.RemoveAt(pos);
                        children.Insert(pos, CreateUI(replace.UI));
                    }
                    break;

                case UIDiff.Remove _:
                    if (!isEmpty)
                        Locate(tail, root).Children.RemoveAt(pos);
                    break;

                // todo: Leaky API, how do I know that Event is handled elsewhere
                case UIDiff.Event _:
                    // do nothing for events because they are raised before applying update in main MVU loop
                    break;
            }
        }

        private static Panel Locate(ImList<int> path, ContentControl root) =>
            path.IsEmpty ? (Panel)root.Content : (Panel)Locate(path.Tail, root).Children[path.Head];

        private static UIElement CreateUI(UI ui)
        {
            if (ui == null)
                throw new ArgumentNullException(nameof(ui));

            if (ui is UI.Text text)
                return new Label { Content = text.Content };

            if (ui is UI.Input input)
            {
                var elem = new TextBox { Text = input.Content };
                var ev = input.Changed.Value;
                elem.TextChanged += (sender, _) => ev.Value(((TextBox)sender).Text);
                return elem;
            }

            if (ui is UI.Button button)
            {
                var elem = new Button { Content = button.Label };

                var ev = button.Clicked.Value;
                elem.Click += (sender, _) => ev.Value(Unit.unit);

                return elem;
            }

            if (ui is UI.Panel panel)
            {
                var orientation = panel.Layout == Layout.Vertical ? Orientation.Vertical : Orientation.Horizontal;
                var elem = new StackPanel { Orientation = orientation };

                var children = panel.Parts.Map(CreateUI);
                children.Do(x => elem.Children.Add(x));

                return elem;
            }

            if (ui is UI.Check check)
            {
                var elem = new CheckBox
                {
                    Content = check.Label,
                    IsChecked = check.IsChecked,
                };

                var ev = check.Changed.Value;
                elem.Checked += (sender, _) => ev.Value(true);
                elem.Unchecked += (sender, _) => ev.Value(false);

                return elem;
            }

            throw new NotSupportedException("The type of UI is not supported: " + ui.GetType());
        }

        //private static readonly Thickness DefaultMargin = new Thickness(2);
        //private static FrameworkElement WithStyles(FrameworkElement elem, ImList<Style> styles)
        //{
        //    elem.Margin = DefaultMargin;
        //    styles.Do(style =>
        //    {
        //        if (style is Style.Width width)
        //            elem.Width = width.Value;
        //        else if (style is Style.Height height)
        //            elem.Height = height.Value;
        //        else if (style is Style.IsEnabled isEnabled)
        //            elem.IsEnabled = isEnabled.Value;
        //        else if (style is Style.Tooltip tooltip)
        //            elem.ToolTip = tooltip.Value;
        //    });
        //    return elem;
        //}

        private static void Update(UI ui, UIElement elem)
        {
            switch (ui)
            {
                case UI.Text text:
                    var label = (Label)elem;
                    label.Content = text.Content;
                    break;

                case UI.Input input:
                    var textBox = (TextBox)elem;
                    textBox.Text = input.Content;
                    break;

                case UI.Button btn:
                    var button = (Button)elem;
                    button.Content = btn.Label;
                    break;

                case UI.Check check:
                    var checkBox = (CheckBox)elem;
                    checkBox.Content = check.Label;
                    checkBox.IsChecked = check.IsChecked;
                    break;
            }
        }
    }
}
