using System;
using System.Windows;
using System.Windows.Controls;
using ImTools;
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
        public void ApplyPatches(ImList<Patch.U> changes) =>
            changes.Apply(x => _rootControl.Dispatcher.Invoke(() => Apply(x, _rootControl)));

        private static void Apply(Patch.U change, ContentControl root)
        {
            switch (change)
            {
                case I<Insert> insert:
                { 
                    var ((pos, tail, isEmpty), ui) = insert.V.V;
                    if (isEmpty)
                        root.Content = CreateElement(ui);
                    else
                        Locate(tail, root).Children.Insert(pos, CreateElement(ui));
                    break;
                }
                case I<Update> update:
                {
                    var ((pos, tail, isEmpty), ui) = update.V.V;
                    var elem = isEmpty ? (UIElement) root.Content : Locate(tail, root).Children[pos];
                    Update(ui, elem);
                    break;
                }
                case I<Replace> replace:
                {
                    var ((pos, tail, isEmpty), ui) = replace.V.V;
                    var elem = CreateElement(ui);
                    if (isEmpty)
                        root.Content = elem;
                    else
                    {
                        var children = Locate(tail, root).Children;
                        children.RemoveAt(pos);
                        children.Insert(pos, elem);
                    }
                    break;
                }
                case I<Remove> remove:
                {
                    var (pos, tail, isEmpty) = remove.V.V;
                    if (!isEmpty)
                        Locate(tail, root).Children.RemoveAt(pos);
                    break;
                }
                // todo: Leaky API, how do I know that Event is handled elsewhere
                case I<Event> _:
                    // do nothing for events because they are raised before applying update in main MVU loop
                    break;
            }
        }

        private static Pnl Locate(ImList<int> path, ContentControl root) =>
            path.IsEmpty ? (Pnl)root.Content : (Pnl)Locate(path.Tail, root).Children[path.Head];

        private static UIElement CreateElement(UI.U ui)
        {
            switch (ui)
            {
                case I<Text> text:
                    return new Label { Content = text.V.V };

                case I<Input> input:
                {
                    var (content, changed) = input.V.V;
                    var tb = new TextBox { Text = content };
                    tb.TextChanged += (sender, _) => changed.Send(((TextBox)sender).Text);
                    return tb;
                }
                case I<Button> button:
                {
                    var (label, clicked) = button.V.V;
                    var b = new Btn { Content = label };
                    b.Click += (sender, _) => clicked.Send(Unit.unit);
                    return b;
                }
                case I<Panel> panel:
                {
                    var (layout, elems) = panel.Value();
                    var orientation = layout == Layout.Vertical ? Orientation.Vertical : Orientation.Horizontal;
                    var p = new StackPanel {Orientation = orientation};
                    elems.Map(CreateElement).Apply(e => p.Children.Add(e));
                    return p;
                }
                case I<Check> check:
                {
                    var (label, isChecked, changed) = check.V.V;
                    var c = new CheckBox { Content = label, IsChecked = isChecked };
                    c.Checked += (s, _) => changed.Send(true);
                    c.Unchecked += (s, _) => changed.Send(false);
                    return c;
                }
                default: 
                    throw new NotSupportedException("The type of UI is not supported: " + ui.GetType());
            }
        }

        private static void Update(UI.U ui, UIElement elem)
        {
            switch (ui)
            {
                case I<Text> t when elem is Label l:
                    l.Content = t.V;
                    break;

                case I<Input> i when elem is TextBox tb:
                    tb.Text = i.V.V.Content;
                    break;

                case I<Button> b when elem is Btn bt:
                    bt.Content = b.V.V.Label;
                    break;

                case I<Check> c when elem is CheckBox cb:
                    (cb.Content, cb.IsChecked) = (c.V.V.Label, c.V.V.IsChecked);
                    break;
            }
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
    }
}
