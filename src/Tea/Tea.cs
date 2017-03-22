using System;
using ImTools;

namespace Tea
{
    public sealed class unit
    {
        public static readonly unit _ = new unit();

        public static unit Ignore<T>(T it) { return _; }

        private unit() { }
    }

    public static class Event
    {
        public static Ref<Ref<Func<TMsg, unit>>> Of<TMsg>(Func<TMsg, unit> evt)
        {
            return Ref.Of(Ref.Of(evt));
        }
    }

    /// Layout for a section of UI components.
    // type Layout = Horizontal | Vertical
    public enum Layout
    {
        Horizontal,
        Vertical
    }

    public abstract class UI
    {
        public readonly string Value;

        private UI(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        //| Text of string
        public class Text : UI
        {
            public Text(string value) : base(value) { }
        }

        //| Input of string* string Event
        public class Input : UI
        {
            public readonly Ref<Ref<Func<string, unit>>> Event;

            public Input(string value, Ref<Ref<Func<string, unit>>> evt) : base(value)
            {
                Event = evt;
            }
        }

        //| Button of string* unit Event
        public class Button : UI
        {
            public readonly Ref<Ref<Func<unit, unit>>> Event;

            public Button(string value, Ref<Ref<Func<unit, unit>>> evt) : base(value)
            {
                Event = evt;
            }
        }

        //| Div of Layout* UI list
        public class Div : UI
        {
            public readonly Layout Layout;
            public readonly ImList<UI> Parts;

            public Div(Layout layout, ImList<UI> parts) : base(null)
            {
                Layout = layout;
                Parts = parts;
            }
        }
    }

    /// UI component update and event redirection.
    public abstract class UIUpdate
    {
        //| InsertUI of int list * UI
        public class Insert : UIUpdate
        {
            public readonly ImList<int> Path;
            public readonly UI UI;

            public Insert(ImList<int> path, UI ui)
            {
                Path = path;
                UI = ui;
            }
        }

        //| UpdateUI of int list * UI
        public class Update : UIUpdate
        {
            public readonly ImList<int> Path;
            public readonly UI UI;

            public Update(ImList<int> path, UI ui)
            {
                Path = path;
                UI = ui;
            }
        }

        //| ReplaceUI of int list * UI
        public class Replace : UIUpdate
        {
            public readonly ImList<int> Path;
            public readonly UI UI;

            public Replace(ImList<int> path, UI ui)
            {
                Path = path;
                UI = ui;
            }
        }

        //| RemoveUI of int list
        public class Remove : UIUpdate
        {
            public readonly ImList<int> Path;

            public Remove(ImList<int> path)
            {
                Path = path;
            }
        }

        //| EventUI of(unit->unit)
        public class Event : UIUpdate
        {
            public readonly Func<unit, unit> Raise;

            public Event(Func<unit, unit> raise)
            {
                Raise = raise;
            }
        }
    }

    /// UI component including a message event.
    //type 'msg UI = {UI:UI;mutable Event:'msg->unit}
    public class UI<TMsg> 
    {
        public readonly UI InternalUI;
        public Func<TMsg, unit> Event;

        public UI(UI internalUI, Func<TMsg, unit> @event)
        {
            InternalUI = internalUI;
            Event = @event;
        }
    }

    /// Simple UI application.
    public class App<TMsg, TModel>
    {
        public readonly TModel Model;
        public readonly Func<TMsg, TModel, TModel> Update;
        public readonly Func<TModel, UI<TMsg>> View;

        public App(TModel model, Func<TMsg, TModel, TModel> update, Func<TModel, UI<TMsg>> view)
        {
            Model = model;
            Update = update;
            View = view;
        }
    }

    /// Native UI interface.
    public interface INativeUI
    {
        void Send(ImList<UIUpdate> uiUpdates);
    }

    public static class UIParts
    {
        public static UI<TMsg> text<TMsg>(string text)
        {
            return new UI<TMsg>(new UI.Text(text), unit.Ignore);
        }

        public static UI<string> input(string text)
        {
            var ev = Event.Of<string>(unit.Ignore);
            var ui = new UI<string>(new UI.Input(text, ev), unit.Ignore);
            Func<string, unit> raise = s => ui.Event(s);
            ev.Value.Swap(raise);
            return ui;
        }

        public static UI<TMsg> button<TMsg>(string text, TMsg msg)
        {
            var ev = Event.Of<unit>(unit.Ignore);
            var ui = new UI<TMsg>(new UI.Button(text, ev), unit.Ignore);
            ev.Value.Swap(_ => ui.Event(msg));
            return ui;
        }

        public static UI<TMsg> div<TMsg>(Layout layout, params UI<TMsg>[] parts)
        {
            var uiParts = ImList<UI>.Empty;

            // add in reverse order to preserve the parts order
            for (var i = parts.Length - 1; i >= 0; i--)
                uiParts = uiParts.Push(parts[i].InternalUI);

            var ui = new UI<TMsg>(new UI.Div(layout, uiParts), unit.Ignore);

            Func<TMsg, unit> raise = msg => ui.Event(msg);

            for (var i = 0; i < parts.Length; i++)
                parts[i].Event = raise;

            return ui;
        }
    }

    public static class UIApp
    {
        /// Returns a new UI component mapping the message event using the given function.
        public static UI<TOutMsg> Map<TInMsg, TOutMsg>(this UI<TInMsg> source, Func<TInMsg, TOutMsg> map)
        {
            var result = new UI<TOutMsg>(source.InternalUI, unit.Ignore);
            source.Event = msg => result.Event(map(msg));
            return result;
        }

        /// Returns a list of UI updates from two UI components.
        public static ImList<UIUpdate> Diff<TMsg1, TMsg2>(this UI<TMsg1> oldUI, UI<TMsg2> newUI)
        {
            return Diff(oldUI.InternalUI, newUI.InternalUI, ImList<int>.Empty, 0, ImList<UIUpdate>.Empty);
        }

        private static ImList<UIUpdate> Diff(UI oldUI, UI newUI, ImList<int> path, int index, ImList<UIUpdate> diffs)
        {
            if (ReferenceEquals(oldUI, newUI))
                return diffs;

            // todo: consolidate same ui handling because it is not different
            if (oldUI is UI.Text && newUI is UI.Text)
            {
                if (oldUI.Value != newUI.Value)
                    diffs = diffs.Push(new UIUpdate.Update(path, newUI));
                return diffs;
            }

            if (oldUI is UI.Button && newUI is UI.Button)
            {
                if (oldUI.Value != newUI.Value)
                    diffs = diffs.Push(new UIUpdate.Update(path, newUI));

                var updateEvent = Update(((UI.Button)oldUI).Event, ((UI.Button)newUI).Event);
                return diffs.Push(new UIUpdate.Event(updateEvent));
            }

            if (oldUI is UI.Input && newUI is UI.Input)
            {
                if (oldUI.Value != newUI.Value)
                    diffs = diffs.Push(new UIUpdate.Update(path, newUI));

                var updateEvent = Update(((UI.Input)oldUI).Event, ((UI.Input)newUI).Event);
                return diffs.Push(new UIUpdate.Event(updateEvent));
            }

            var oldDiv = oldUI as UI.Div;
            var newDiv = newUI as UI.Div;

            if (oldDiv != null && newDiv != null)
            {
                // if layout changed then fully replace
                if (oldDiv.Layout != newDiv.Layout)
                    return diffs.Push(new UIUpdate.Replace(path, newUI));

                var oldChildren = oldDiv.Parts;
                var newChildren = newDiv.Parts;

                // if both empty
                if (oldChildren.IsEmpty && newChildren.IsEmpty)
                    return diffs;

                // for each new child UI do insert
                if (oldChildren.IsEmpty)
                    return newChildren.To(index, diffs, (ui, i, _) => _.Push(new UIUpdate.Insert(path.Push(i), ui)));

                // remove old ui children
                if (newChildren.IsEmpty)
                    return oldChildren.To(index, diffs, (ui, i, _) => _.Push(new UIUpdate.Remove(path.Push(i))));

                // diff the first items, then recursively the rest 
                diffs = Diff(oldChildren.Head, newChildren.Head, path.Push(index), 0, diffs);

                return Diff(
                    new UI.Div(oldDiv.Layout, oldChildren.Tail),
                    new UI.Div(oldDiv.Layout, newChildren.Tail),
                    path, index + 1, diffs);
            }

            // otherwise just replace
            return diffs.Push(new UIUpdate.Replace(path, newUI));
        }

        // Point first ref to second ref value.value
        private static Func<unit, unit> Update<T>(Ref<Ref<T>> ref1, Ref<Ref<T>> ref2)
            where T : class
        {
            return _ =>
            {
                //let ev = !e1 in ev:=!(!e2); e2:=ev
                var ref1Value = ref1.Value;
                ref1Value.Swap(ref2.Value.Value);
                ref2.Swap(ref1Value);
                return unit._;
            };
        }

        // Returns a UI application from a UI model, update and view.
        public static App<TMsg, TModel> App<TMsg, TModel>(
            TModel model, 
            Func<TMsg, TModel, TModel> update,
            Func<TModel, UI<TMsg>> view)
        {
            return new App<TMsg, TModel>(model, update, view);
        }

        // Runs a UI application given a native UI.
        public static void Run<TMsg, TModel>(INativeUI nativeUI, App<TMsg, TModel> app)
        {
            Func<TModel, UI<TMsg>, TMsg, unit> handleRecursively = null;
            handleRecursively = (model, view, message) =>
            {
                var newModel = app.Update(message, model);
                var newUI = app.View(newModel);

                newUI.Event = msg => handleRecursively(newModel, newUI, msg);

                var uiUpdates = view.Diff(newUI);
                uiUpdates.To(unit._, (update, _) => (update as UIUpdate.Event)?.Raise(unit._));

                nativeUI.Send(uiUpdates);

                return unit._;
            };

            // Render and insert intial UI from the model
            var initialUI = app.View(app.Model);
            initialUI.Event = msg => handleRecursively(app.Model, initialUI, msg);
            nativeUI.Send(ImList<UIUpdate>.Empty.Push(new UIUpdate.Insert(ImList<int>.Empty, initialUI.InternalUI)));
        }
    }
}
