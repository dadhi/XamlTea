using System;
using ImTools;
using static Tea.Unit;
using static Tea.ImToolsExt;
using static Tea.UIDiff;
// ReSharper disable InconsistentNaming
#pragma warning disable 659

namespace Tea
{
    public delegate void Event<in T>(T msg);

    public static class Event
    {
        public static void Empty<TMsg>(TMsg _) { }
        public static Ref<Ref<Event<T>>> Of<T>(Event<T> send) => Ref.Of(Ref.Of(send));
    }

    //public abstract class Style
    //{
    //    public class Of<TValue> : Style
    //    {
    //        public readonly TValue Value;
    //        public Of(TValue value) { Value = value; }
    //        public override bool Equals(object obj)
    //        {
    //            var other = obj as Of<TValue>;
    //            return other != null && Equals(other.Value, Value);
    //        }
    //    }

    //    public class Width : Of<int> { public Width(int value) : base(value) { } }
    //    public class Height : Of<int> { public Height(int value) : base(value) { } }

    //    public class IsEnabled : Of<bool>
    //    {
    //        public static readonly IsEnabled Enabled = new IsEnabled(true);
    //        public static readonly IsEnabled Disabled = new IsEnabled(false);
    //        private IsEnabled(bool value) : base(value) { }
    //    }
    //    public class Tooltip : Of<string> { public Tooltip(string value) : base(value) { } }
    //}

    //public static class Styles
    //{
    //    public static ImList<Style> style(params Style[] styles) => styles.AsList();

    //    public static Style width(int n) => new Style.Width(n);
    //    public static Style height(int n) => new Style.Height(n);
    //    public static Style isEnabled(bool enabled) => enabled ? Style.IsEnabled.Enabled : Style.IsEnabled.Disabled;
    //    public static Style tooltip(string text) => new Style.Tooltip(text);

    //    public static ImList<(Style, Style)> PrependDiffs(this ImList<Style> source, ImList<Style> other)
    //    {
    //        if (source.IsEmpty && other.IsEmpty)
    //            return ImList<(Style, Style)>.Empty;

    //        if (other.IsEmpty)
    //            return source.Map(s => (s, default(Style)));

    //        if (source.IsEmpty)
    //            return other.Map(s => (default(Style), s));

    //        var resultDiff = source.Fold(ImList<(Style, Style)>.Empty, (sourceStyle, sourceDiff) =>
    //        {
    //            var s2 = default(Style);
    //            other = other.Fold(ImList<Style>.Empty, (otherStyle, otherDiff) =>
    //            {
    //                if (otherStyle.GetType() != sourceStyle.GetType())
    //                    return otherDiff.Prepend(otherStyle);

    //                s2 = otherStyle;
    //                return otherDiff;
    //            });

    //            if (s2 == null)
    //                return sourceDiff.Prepend((sourceStyle, default(Style)));

    //            if (s2 != null && sourceStyle != s2 && !s2.Equals(sourceStyle))
    //                return sourceDiff.Prepend((sourceStyle, s2));

    //            return sourceDiff;
    //        });

    //        if (other.IsEmpty)
    //            return resultDiff;

    //        return other.Fold(resultDiff, (style, diff) => diff.Prepend((default(Style), style)));
    //    }
    //}

    public enum Layout { Horizontal, Vertical }

    public abstract class UI
    {
        public class Text : UI
        {
            public readonly string Content;
            public Text(string text) => Content = text;

            public override string ToString() => $"Text('{Content}')";
        }

        public class Input : UI
        {
            public readonly string Content;
            public readonly Ref<Ref<Event<string>>> Changed;

            public Input(string content, Ref<Ref<Event<string>>> changed)
            {
                Content = content;
                Changed = changed;
            }

            public override string ToString() => $"Input('{Content}')";
        }

        public class Button : UI
        {
            public readonly string Label;
            public readonly Ref<Ref<Event<Unit>>> Clicked;

            public Button(string label, Ref<Ref<Event<Unit>>> clicked)
            {
                Label = label;
                Clicked = clicked;
            }

            public override string ToString() => $"Button('{Label}')";
        }

        public class Check : UI
        {
            public readonly string Label;
            public readonly bool IsChecked;
            public readonly Ref<Ref<Event<bool>>> Changed;

            public Check(string label, bool isChecked, Ref<Ref<Event<bool>>> changed)
            {
                Label = label;
                IsChecked = isChecked;
                Changed = changed;
            }

            public override string ToString() => $"Check({IsChecked},'{Label}')";
        }

        public class Panel : UI
        {
            public readonly Layout Layout;
            public readonly ImList<UI> Parts;

            public Panel(Layout layout, ImList<UI> parts)
            {
                Layout = layout;
                Parts = parts;
            }

            public override string ToString() => (Layout == Layout.Vertical ? "Col(" : "Row(") + Parts + ")";
        }
    }

    /// UI component update and event redirection.
    public abstract class UIDiff
    {
        public readonly ImList<int> Path;
        protected UIDiff(ImList<int> path) => Path = path;

        public class Insert : UIDiff
        {
            public readonly UI UI;
            public Insert(ImList<int> path, UI ui) : base(path) => UI = ui;
        }

        public class Update : UIDiff
        {
            public readonly UI UI;
            public Update(ImList<int> path, UI ui) : base(path) => UI = ui;
        }

        public class Replace : UIDiff
        {
            public readonly UI UI;
            public Replace(ImList<int> path, UI ui) : base(path) => UI = ui;
        }

        public class Remove : UIDiff
        {
            public Remove(ImList<int> path) : base(path) { }
        }

        public class Event : UIDiff
        {
            public readonly Action<Unit> Send;
            public Event(Action<Unit> send) : base(ImList<int>.Empty) => Send = send;
        }
    }

    /// UI component including a message event.
    public class UI<TMsg>
    {
        public readonly UI BaseUI;
        public Action<TMsg> Send;

        public UI(UI baseUi, Action<TMsg> send)
        {
            BaseUI = baseUi;
            Send = send;
        }
    }

    /// <summary>Base interface for component with Update, View but without Commands, Subscriptions.</summary>
    public interface IComponent<T> where T : IComponent<T>
    {
        T Update(IMsg<T> msg);

        UI<IMsg<T>> View();
    }

    /// <summary>Marker interface to allow boilerplate removal.</summary>
    /// <typeparam name="T">Component type</typeparam>
    // ReSharper disable once UnusedTypeParameter
    public interface IMsg<T> { }

    public class ItemChanged<TItem, THolder> : IMsg<THolder>
    {
        public int Index { get; }
        public IMsg<TItem> Msg { get; }

        public ItemChanged(int index, IMsg<TItem> msg)
        {
            Index = index;
            Msg = msg;
        }
    }

    public static class Component
    {
        public static UI<IMsg<THolder>> ViewIn<TItem, THolder>(this TItem item, int itemIndex)
            where TItem : IComponent<TItem>
            where THolder : IComponent<THolder> =>
            item.View().MapMsg(msg => msg.Lift<TItem, THolder>(itemIndex));

        public static UI<IMsg<THolder>> ViewIn<TItem, THolder>(this TItem item, THolder hereOnlyForTypeInference,
            int itemIndex = 0)
            where TItem : IComponent<TItem> => 
            item.View().MapMsg(msg => msg.Lift<TItem, THolder>(itemIndex));

        public static IMsg<THolder> Lift<TItem, THolder>(this IMsg<TItem> itemMsg, int itemIndex) =>
            new ItemChanged<TItem, THolder>(itemIndex, itemMsg);
    }

    public interface INativeUI
    {
        void ApplyDiffs(ImList<UIDiff> diffs);
    }

    public static class UIParts
    {
        public static UI<TMsg> text<TMsg>(string text) =>
            new UI<TMsg>(new UI.Text(text), Event.Empty);

        public static UI<TMsg> input<TMsg>(string text, Func<string, TMsg> changed)
        {
            var ev = Event.Of<string>(Event.Empty);
            var ui = new UI<TMsg>(new UI.Input(text, ev), Event.Empty);
            ev.Value.Swap(s => ui.Send(changed(s)));
            return ui;
        }

        public static UI<TMsg> button<TMsg>(string text, TMsg clicked)
        {
            var ev = Event.Of<Unit>(Event.Empty);
            var ui = new UI<TMsg>(new UI.Button(text, ev), Event.Empty);
            ev.Value.Swap(_ => ui.Send(clicked));
            return ui;
        }

        public static UI<TMsg> check<TMsg>(string text, bool isChecked, Func<bool, TMsg> changed)
        {
            var ev = Event.Of<bool>(Event.Empty);
            var ui = new UI<TMsg>(new UI.Check(text, isChecked, ev), Event.Empty);
            ev.Value.Swap(check => ui.Send(changed(check)));
            return ui;
        }

        public static UI<TMsg> panel<TMsg>(Layout layout, ImList<UI<TMsg>> uis)
        {
            var panel = new UI<TMsg>(new UI.Panel(layout, uis.Map(x => x.BaseUI)), Event.Empty);
            void Send(TMsg m) => panel.Send(m);
            uis.Do(x => x.Send = Send);
            return panel;
        }

        public static UI<TMsg> row<TMsg>(ImList<UI<TMsg>> uis) =>
            panel(Layout.Horizontal, uis);

        public static UI<TMsg> row<TMsg>(params UI<TMsg>[] uis) => row(list(uis));

        public static UI<TMsg> column<TMsg>(ImList<UI<TMsg>> uis) =>
            panel(Layout.Vertical, uis);

        public static UI<TMsg> column<TMsg>(params UI<TMsg>[] uis) => column(list(uis));
    }

    public static class UIApp
    {
        ///<summary>Returns a new UI component mapping the message event using the given function.</summary>
        public static UI<THolderMsg> MapMsg<TItemMsg, THolderMsg>(
            this UI<TItemMsg> source, Func<TItemMsg, THolderMsg> map)
        {
            var result = new UI<THolderMsg>(source.BaseUI, Event.Empty);
            source.Send = msg => result.Send(map(msg));
            return result;
        }

        ///<summary>Returns a list of UI updates from two UI components.
        /// To ensure correct insert and removal sequence where the insert/remove index are existing.</summary> 
        public static ImList<UIDiff> Diff<TMsg1, TMsg2>(this UI<TMsg1> ui, UI<TMsg2> newUI) =>
            Diff(ImList<UIDiff>.Empty, ui.BaseUI, newUI.BaseUI, path: ImList<int>.Empty, pos: 0);

        private static ImList<UIDiff> Diff(this ImList<UIDiff> diffs, UI ui, UI newUI, ImList<int> path, int pos)
        {
            if (ReferenceEquals(ui, newUI))
                return diffs;

            if (ui is UI.Text text && newUI is UI.Text newText)
                return text.Content == newText.Content ? diffs : new Update(path, newUI).Cons(diffs);

            if (ui is UI.Button btn && newUI is UI.Button newBtn)
            {
                if (btn.Label != newBtn.Label)
                    diffs = new Update(path, newBtn).Cons(diffs);
                return new UIDiff.Event(btn.Clicked.Swap(newBtn.Clicked)).Cons(diffs);
            }

            if (ui is UI.Input input && newUI is UI.Input newInput)
            {
                if (input.Content != newInput.Content)
                    diffs = new Update(path, newInput).Cons(diffs);
                return new UIDiff.Event(input.Changed.Swap(newInput.Changed)).Cons(diffs);
            }

            if (ui is UI.Check check && newUI is UI.Check newCheck)
                return new UIDiff.Event(check.Changed.Swap(newCheck.Changed))
                    .Cons(check.IsChecked != newCheck.IsChecked || check.Label != newCheck.Label 
                        ? new Update(path, newCheck).Cons(diffs) 
                        : diffs);

            if (ui is UI.Panel panel && newUI is UI.Panel newPanel)
                return panel.Layout == newPanel.Layout
                    ? diffs.Diff(panel.Parts, newPanel.Parts, path, pos)
                    : new Replace(path, newPanel).Cons(diffs);

            return new Replace(path, newUI).Cons(diffs);
        }

        private static ImList<UIDiff> Diff(this ImList<UIDiff> diffs,
            ImList<UI> oldParts, ImList<UI> newParts, ImList<int> path, int pos)
        {
            if (oldParts.IsEmpty && newParts.IsEmpty)
                return diffs;

            if (oldParts.IsEmpty)
                return newParts.Fold(diffs, (ui, i, d) => new Insert((pos + i).Cons(path), ui).Cons(d));

            if (newParts.IsEmpty)
                return oldParts.Fold(diffs, (_, i, d) => new Remove((pos + i).Cons(path)).Cons(d));

            return  diffs
                .Diff(oldParts.Head, newParts.Head, pos.Cons(path), 0)
                .Diff(oldParts.Tail, newParts.Tail, path, pos + 1);
        }

        // Point first ref to second ref value.value
        private static Action<Unit> Swap<T>(this Ref<Ref<T>> e1, Ref<Ref<T>> e2) where T : class => _ =>
        {
            //let ev = !e1 in ev:=!(!e2); e2:=ev
            var e1Val = e1.Value;
            e1Val.Swap(e2.Value.Value);
            e2.Swap(e1Val);
        };

        /// <summary>Runs Model-View-Update loop, e.g. Init->View->Update->View->Update->View... </summary>
        public static void Run<T>(INativeUI nativeUI, IComponent<T> component) where T : IComponent<T>
        {
            void UpdateViewLoop(IComponent<T> comp, UI<IMsg<T>> ui, IMsg<T> message)
            {
                var newComp = comp.Update(message);
                var newUI = newComp.View();
                newUI.Send = msg => UpdateViewLoop(newComp, newUI, msg);

                var diffs = ui.Diff(newUI);

                diffs.Do(d => (d as UIDiff.Event)?.Send(unit));

                nativeUI.ApplyDiffs(diffs);
            }

            // Render and insert initial UI from the model
            var initialUI = component.View();
            initialUI.Send = msg => UpdateViewLoop(component, initialUI, msg);
            nativeUI.ApplyDiffs(new Insert(ImList<int>.Empty, initialUI.BaseUI).Cons<UIDiff>());
        }
    }
}
