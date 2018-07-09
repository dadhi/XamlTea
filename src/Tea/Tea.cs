// ReSharper disable InconsistentNaming
#pragma warning disable 659

namespace Tea
{
    using System;
    using ImTools;
    using static ImToolsExt;
    using static UIChange;

    // todo: can we do like this?
    // public sealed class MessageR<M> : Case<MessageR<M>, Ref<Ref<Action<M>>>> { }
    public struct MessageRef<M>
    {
        public Ref<Ref<Action<M>>> Ref;
        public MessageRef(Ref<Ref<Action<M>>> r) => Ref = r;
    }

    public static class Message
    {
        public static void Empty<M>(M _) { }
        
        public static MessageRef<M> Ref<M>(Action<M> send) => 
            new MessageRef<M>(ImTools.Ref.Of(ImTools.Ref.Of(send)));
        
        public static MessageRef<M> EmptyRef<M>() => Ref<M>(Empty);

        public static void Set<M>(this MessageRef<M> x, Action<M> send) => x.Ref.Value.Swap(send);

        public static void Send<M>(this MessageRef<M> x, M m) => x.Ref.Value.Value(m);

        public static Action Updater<M>(this MessageRef<M> a, MessageRef<M> b) => () =>
        {
            var ar = a.Ref.Value;
            ar.Swap(b.Ref.Value.Value);
            b.Ref.Swap(ar);
        };
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
    //    public static Style tooltip(string label) => new Style.Tooltip(label);

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

    public sealed class UI : Union<UI, Text.I, Input.I, Button.I, Check.I, Panel> { }
    public sealed class Text   : Case<Text, string> { }
    public sealed class Input  : Case<Input, (string Content, MessageRef<string> Changed)> { }
    public sealed class Button : Case<Button, (string Label, MessageRef<Unit> Clicked)> { }
    public sealed class Check  : Case<Check, (string Label, bool IsChecked, MessageRef<bool> Changed)> { }
    public sealed class Panel  : Rec<Panel, (Layout Layout, ImList<UI.U> Elements)> { }

    /// UI component update and event redirection.
    public abstract class UIChange
    {
        public readonly ImList<int> Path;
        protected UIChange(ImList<int> path) => Path = path;

        public class Insert : UIChange
        {
            public readonly UI.U UI;
            public Insert(ImList<int> path, UI.U ui) : base(path) => UI = ui;
        }

        public class Update : UIChange
        {
            public readonly UI.U UI;
            public Update(ImList<int> path, UI.U ui) : base(path) => UI = ui;
        }

        public class Replace : UIChange
        {
            public readonly UI.U UI;
            public Replace(ImList<int> path, UI.U ui) : base(path) => UI = ui;
        }

        public class Remove : UIChange
        {
            public Remove(ImList<int> path) : base(path) { }
        }

        public class Message : UIChange
        {
            public readonly Action Apply;
            public Message(Action apply) : base(ImList<int>.Empty) => Apply = apply;
        }
    }

    /// UI with message M.
    public class UI<M>
    {
        public readonly UI.U Element;
        public Action<M> Send;
        public UI(UI.U element, Action<M> send) => (Element, Send) = (element, send);
    }

    /// Base interface for component with Update, View but without Commands, Subscriptions.
    public interface IComponent<T> where T : IComponent<T>
    {
        T Update(IMessage<T> message);
        UI<IMessage<T>> View();
    }

    /// Marker interface for boilerplate removal.
    // ReSharper disable once UnusedTypeParameter
    public interface IMessage<T> { }

    public struct ChildChanged<TChild, TParent> : IMessage<TParent>
    {
        public readonly int Index;
        public readonly IMessage<TChild> Message;
        public ChildChanged(int index, IMessage<TChild> message) => (Index, Message) = (index, message);
    }

    public static class Component
    {
        public static UI<IMessage<TParent>> In<TChild, TParent>(this TChild child, int childIndex)
            where TChild : IComponent<TChild>
            where TParent : IComponent<TParent> =>
            child.View().Map(m => m.Lift<TChild, TParent>(childIndex));

        public static UI<IMessage<TParent>> In<TChild, TParent>(this TChild child, TParent _onlyForInference, int childIndex = 0)
            where TChild : IComponent<TChild> =>
            child.View().Map(m => m.Lift<TChild, TParent>(childIndex));

        public static IMessage<TParent> Lift<TChild, TParent>(this IMessage<TChild> childMessage, int childIndex) =>
            new ChildChanged<TChild, TParent>(childIndex, childMessage);
    }

    public interface INativeUI
    {
        void ApplyChanges(ImList<UIChange> changes);
    }

    public static class UIElements
    {
        public static UI<M> text<M>(string text) =>
            new UI<M>(UI.Of(Text.Of(text)), Message.Empty);

        public static UI<M> text<M>(object textObj) => text<M>("" + textObj);

        public static UI<M> input<M>(string text, Func<string, M> onChange)
        {
            var m = Message.EmptyRef<string>();
            return new UI<M>(UI.Of(Input.Of((text, m))), Message.Empty)
                .Do(x => m.Set(s => x.Send(onChange(s))));
        }

        public static UI<M> button<M>(string label, Func<M> onClick)
        {
            var m = Message.EmptyRef<Unit>();
            return new UI<M>(UI.Of(Button.Of((label, m))), Message.Empty)
                .Do(x => m.Set(_ => x.Send(onClick())));
        }

        public static UI<M> button<M>(string label, M onClickMessage) =>
            button(label, () => onClickMessage);

        public static UI<M> check<M>(string label, bool isChecked, Func<bool, M> onCheck)
        {
            var m = Message.EmptyRef<bool>();
            return new UI<M>(UI.Of(Check.Of((label, isChecked, m))), Message.Empty)
                .Do(x => m.Set(b => x.Send(onCheck(b))));
        }

        public static UI<M> panel<M>(Layout layout, ImList<UI<M>> elements)
        {
            var ui = new UI<M>(UI.Of(Panel.Of((layout, elements.Map(x => x.Element)))), Message.Empty);
            void Send(M m) => ui.Send(m);
            elements.Apply(x => x.Send = Send);
            return ui;
        }

        public static UI<M> row<M>(ImList<UI<M>> uis) => panel(Layout.Horizontal, uis);

        public static UI<M> row<M>(params UI<M>[] kids) => row(list(kids));

        public static UI<M> column<M>(ImList<UI<M>> kids) => panel(Layout.Vertical, kids);

        public static UI<M> column<M>(params UI<M>[] kids) => column(list(kids));
    }

    public static class UIApplication
    {
        /// Returns a new UI component mapping the message using the given function.
        public static UI<B> Map<A, B>(this UI<A> source, Func<A, B> map)
        {
            var target = new UI<B>(source.Element, Message.Empty);
            void Send(A a) => target.Send(map(a));
            source.Send = Send;
            return target;
        }

        /// Returns a list of UI updates from two UI components.
        /// To ensure correct insert and removal sequence where the insert/remove index are existing.
        public static ImList<UIChange> Diff<M1, M2>(this UI<M1> a, UI<M2> b) =>
            Diff(ImList<UIChange>.Empty, a.Element, b.Element, path: ImList<int>.Empty, pos: 0);

        private static ImList<UIChange> Diff(this ImList<UIChange> changes, UI.U a, UI.U b, ImList<int> path, int pos)
        {
            if (ReferenceEquals(a, b))
                return changes;

            if (a is I<Text.I> textA && b is I<Text.I> textB)
                return textA.V.V == textB.V.V ? changes : changes.Prepend(new Update(path, b));

            if (a is I<Button.I> buttonA && b is I<Button.I> buttonB)
            {
                if (buttonA.V.V.Label != buttonB.V.V.Label)
                    changes = changes.Prepend(new Update(path, b));
                return changes.Prepend(new UIChange.Message(buttonA.V.V.Clicked.Updater(buttonB.V.V.Clicked)));
            }

            if (a is I<Input.I> inputA && b is I<Input.I> inputB)
            {
                if (inputA.V.V.Content != inputB.V.V.Content)
                    changes = changes.Prepend(new Update(path, b));
                return changes.Prepend(new UIChange.Message(inputA.V.V.Changed.Updater(inputB.V.V.Changed)));
            }

            // we can do this a different fluent way
            if (a is I<Check.I> checkA && b is I<Check.I> checkB)
                return new UIChange.Message(checkA.V.V.Changed.Updater(checkB.V.V.Changed))
                    .Cons(checkA.V.V.IsChecked == checkB.V.V.IsChecked && checkA.V.V.Label == checkB.V.V.Label 
                        ? changes : new Update(path, b).Cons(changes));

            if (a is I<Panel> panelA && b is I<Panel> panelB)
                return panelA.V.V.Layout == panelB.V.V.Layout
                    ? changes.Diff(panelA.V.V.Elements, panelB.V.V.Elements, path, pos)
                    : changes.Prepend(new Replace(path, b));

            return changes.Prepend(new Replace(path, b));
        }

        private static ImList<UIChange> Diff(this ImList<UIChange> changes, ImList<UI.U> a, ImList<UI.U> b, ImList<int> path, int pos)
        {
            if (a.IsEmpty && b.IsEmpty)
                return changes;

            if (a.IsEmpty)
                return b.Fold(changes, (ui, i, tail) => new Insert((pos + i).Cons(path), ui).Cons(tail));

            if (b.IsEmpty)
                return a.Fold(changes, (_, i, tail) => new Remove((pos + i).Cons(path)).Cons(tail));

            return  changes
                .Diff(a.Head, b.Head, pos.Cons(path), 0)
                .Diff(a.Tail, b.Tail, path, pos + 1);
        }

        /// <summary>Runs Model-View-Update loop, e.g. Init->View->Update->View->Update->View... </summary>
        public static void Run<T>(INativeUI nativeUI, IComponent<T> application) where T : IComponent<T>
        {
            // Render and insert initial UI from the model
            var initialUI = application.View();
            initialUI.Send = m => UpdateViewLoop(application, initialUI, m);
            nativeUI.ApplyChanges(new Insert(ImList<int>.Empty, initialUI.Element).Cons<UIChange>());

            void UpdateViewLoop(IComponent<T> app, UI<IMessage<T>> ui, IMessage<T> msg)
            {
                var newModel = app.Update(msg);
                var newUI = newModel.View();
                newUI.Send = m => UpdateViewLoop(newModel, newUI, m);
                var changes = ui.Diff(newUI);
                changes.Apply(x => (x as UIChange.Message)?.Apply());
                nativeUI.ApplyChanges(changes);
            }
        }
    }
}
