using System;
using System.Windows;
using System.Windows.Controls;
using ImTools;
using static Tea.UIChange;
using Pnl = System.Windows.Controls.Panel;
using Btn = System.Windows.Controls.Button;

namespace Tea.Wpf
{
    public class WpfUI : INativeUI
    {
        /// <summary>Initializes the UI bound to provided content control.</summary>
        public static INativeUI Create(ContentControl root) => new WpfUI(root);

        private readonly ContentControl _rootControl;
        private WpfUI(ContentControl rootControl) => _rootControl = rootControl;

        /// <summary>Applies the updates.</summary>
        public void ApplyChanges(ImList<UIChange> changes) =>
            changes.Apply(x => _rootControl.Dispatcher.Invoke(() => Apply(x, _rootControl)));

        private static void Apply(UIChange change, ContentControl root)
        {
            var (pos, tail, isEmpty) = change.Path;
            switch (change)
            {
                case Insert insert:
                    if (isEmpty)
                        root.Content = CreateUI(insert.UI);
                    else
                        Locate(tail, root).Children.Insert(pos, CreateUI(insert.UI));
                    break;

                case Update update:
                    var elem = isEmpty ? (UIElement)root.Content : Locate(tail, root).Children[pos];
                    Update(update.UI, elem);
                    break;

                case Replace replace:
                    if (isEmpty)
                        root.Content = CreateUI(replace.UI);
                    else
                    {
                        var children = Locate(tail, root).Children;
                        children.RemoveAt(pos);
                        children.Insert(pos, CreateUI(replace.UI));
                    }
                    break;

                case Remove _:
                    if (!isEmpty)
                        Locate(tail, root).Children.RemoveAt(pos);
                    break;

                // todo: Leaky API, how do I know that Event is handled elsewhere
                case UIChange.Message _:
                    // do nothing for events because they are raised before applying update in main MVU loop
                    break;
            }
        }

        private static Pnl Locate(ImList<int> path, ContentControl root) =>
            path.IsEmpty ? (Pnl)root.Content : (Pnl)Locate(path.Tail, root).Children[path.Head];

        private static UIElement CreateUI(UI.U ui)
        {
            switch (ui)
            {
                case I<Text.I> text:
                    return new Label { Content = text.V.V };

                case I<Input.I> input:
                    var (content, changed) = input.V.V;
                    var tb = new TextBox { Text = content };
                    tb.TextChanged += (sender, _) => changed.Send(((TextBox)sender).Text);
                    return tb;
                
                case I<Button.I> button:
                    var (label, clicked) = button.V.V;
                    var b = new Btn { Content = label };
                    b.Click += (sender, _) => clicked.Send(Unit.unit);
                    return b;
                
                case I<Panel> panel:
                    var (layout, elems) = panel.Val();
                    var orientation = layout == Layout.Vertical ? Orientation.Vertical : Orientation.Horizontal;
                    var p = new StackPanel { Orientation = orientation };
                    elems.Map(CreateUI).Apply(e => p.Children.Add(e));
                    return p;
                
                case I<Check.I> check:
                    var (checkLabel, isChecked, checkChanged) = check.V.V;
                    var c = new CheckBox { Content = checkLabel, IsChecked = isChecked };
                    c.Checked += (s, _) => checkChanged.Send(true);
                    c.Unchecked += (s, _) => checkChanged.Send(false);
                    return c;
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

        private static void Update(UI.U ui, UIElement elem)
        {
            switch (ui)
            {
                case I<Text.I> t when elem is Label l:
                    l.Content = t.V;
                    break;

                case I<Input.I> i when elem is TextBox tb:
                    tb.Text = i.V.V.Content;
                    break;

                case I<Button.I> b when elem is Btn bt:
                    bt.Content = b.V.V.Label;
                    break;

                case I<Check.I> c when elem is CheckBox cb:
                    (cb.Content, cb.IsChecked) = (c.V.V.Label, c.V.V.IsChecked);
                    break;
            }
        }
    }
}
