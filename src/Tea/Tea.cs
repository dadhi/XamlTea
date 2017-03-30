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

        public class Text : UI
        {
            public Text(string value) : base(value) { }
        }

        public class Input : UI
        {
            public readonly Ref<Ref<Func<string, unit>>> Event;

            public Input(string value, Ref<Ref<Func<string, unit>>> ev) : base(value)
            {
                Event = ev;
            }
        }

        public class Button : UI
        {
            public readonly Ref<Ref<Func<unit, unit>>> Event;

            public Button(string text, Ref<Ref<Func<unit, unit>>> ev) : base(text)
            {
                Event = ev;
            }
        }

        public class CheckBox : UI
        {
            public readonly bool IsChecked;
            public readonly Ref<Ref<Func<bool, unit>>> Event;

            public CheckBox(string text, bool isChecked, Ref<Ref<Func<bool, unit>>> ev) : base(text)
            {
                IsChecked = isChecked;
                Event = ev;
            }
        }

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
        public readonly Func<TModel, TMsg, TModel> Update;
        public readonly Func<TModel, UI<TMsg>> View;

        public App(TModel model, Func<TModel, TMsg, TModel> update, Func<TModel, UI<TMsg>> view)
        {
            Model = model;
            Update = update;
            View = view;
        }
    }

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

        public static UI<TMsg> input<TMsg>(string text, Func<string, TMsg> msg)
        {
            var ev = Event.Of<string>(unit.Ignore);
            var ui = new UI<TMsg>(new UI.Input(text, ev), unit.Ignore);
            ev.Value.Swap(s => ui.Event(msg(s)));
            return ui;
        }

        public static UI<TMsg> button<TMsg>(string text, TMsg msg)
        {
            var ev = Event.Of<unit>(unit.Ignore);
            var ui = new UI<TMsg>(new UI.Button(text, ev), unit.Ignore);
            ev.Value.Swap(_ => ui.Event(msg));
            return ui;
        }

        public static UI<TMsg> checkbox<TMsg>(string text, bool isChecked, Func<bool, TMsg> msg)
        {
            var ev = Event.Of<bool>(unit.Ignore);
            var ui = new UI<TMsg>(new UI.CheckBox(text, isChecked, ev), unit.Ignore);
            ev.Value.Swap(b => ui.Event(msg(b)));
            return ui;
        }

        public static UI<TMsg> div<TMsg>(Layout layout, params UI<TMsg>[] parts)
        {
            var uiParts = ImList<UI>.Empty;

            // add in reverse order to preserve the parts order
            for (var i = parts.Length - 1; i >= 0; i--)
                uiParts = uiParts.Prep(parts[i].InternalUI);

            var ui = new UI<TMsg>(new UI.Div(layout, uiParts), unit.Ignore);

            Func<TMsg, unit> raise = msg => ui.Event(msg);
            for (var i = 0; i < parts.Length; i++)
                parts[i].Event = raise;

            return ui;
        }
    }

    public static class UIApp
    {
        // todo: May be combined with the view to don't write it each time
        /// Returns a new UI component mapping the message event using the given function.
        public static UI<TMsg> MapMsg<TSubMsg, TMsg>(this UI<TSubMsg> source, Func<TSubMsg, TMsg> map)
        {
            var result = new UI<TMsg>(source.InternalUI, unit.Ignore);
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

            // todo: consolidate same ui handling because it is not so different
            if (oldUI is UI.Text && newUI is UI.Text)
            {
                if (oldUI.Value != newUI.Value)
                    diffs = diffs.Prep(new UIUpdate.Update(path, newUI));
                return diffs;
            }

            if (oldUI is UI.Button && newUI is UI.Button)
            {
                if (oldUI.Value != newUI.Value)
                    diffs = diffs.Prep(new UIUpdate.Update(path, newUI));

                var updateEvent = UpdateEvent(((UI.Button)oldUI).Event, ((UI.Button)newUI).Event);
                return diffs.Prep(new UIUpdate.Event(updateEvent));
            }

            if (oldUI is UI.Input && newUI is UI.Input)
            {
                if (oldUI.Value != newUI.Value)
                    diffs = diffs.Prep(new UIUpdate.Update(path, newUI));

                var updateEvent = UpdateEvent(((UI.Input)oldUI).Event, ((UI.Input)newUI).Event);
                return diffs.Prep(new UIUpdate.Event(updateEvent));
            }

            var oldDiv = oldUI as UI.Div;
            var newDiv = newUI as UI.Div;

            if (oldDiv != null && newDiv != null)
            {
                // if layout changed then fully replace
                if (oldDiv.Layout != newDiv.Layout)
                    return diffs.Prep(new UIUpdate.Replace(path, newDiv));

                var oldParts = oldDiv.Parts;
                var newParts = newDiv.Parts;

                // if both empty
                if (oldParts.IsEmpty && newParts.IsEmpty)
                    return diffs;

                // for each new child UI do insert
                if (oldParts.IsEmpty)
                    return newParts.To(diffs, 
                        (ui, i, _) => _.Prep(new UIUpdate.Insert(path.Prep(index + i), ui)))
                        .Reverse();

                // remove old ui children
                if (newParts.IsEmpty)
                    return oldParts.To(diffs, 
                        (ui, i, _) => _.Prep(new UIUpdate.Remove(path.Prep(index + i))));

                // diff the first items, then recursively the rest 
                diffs = Diff(oldParts.Head, newParts.Head, path.Prep(index), 0, diffs);

                return Diff(
                    new UI.Div(oldDiv.Layout, oldParts.Tail),
                    new UI.Div(oldDiv.Layout, newParts.Tail),
                    path, index + 1, diffs);
            }

            // otherwise just replace
            return diffs.Prep(new UIUpdate.Replace(path, newUI));
        }

        // Point first ref to second ref value.value
        private static Func<unit, unit> UpdateEvent<T>(Ref<Ref<T>> ref1, Ref<Ref<T>> ref2)
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
            Func<TModel, TMsg, TModel> update,
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
                var newModel = app.Update(model, message);
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
            nativeUI.Send(ImList<UIUpdate>.Empty.Prep(new UIUpdate.Insert(ImList<int>.Empty, initialUI.InternalUI)));
        }
    }
}
