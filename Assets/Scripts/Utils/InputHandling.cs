using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EventDirection = Shared.Directions.Direction;


namespace InputHandling
{

    public enum EventType
    {
        ATTACK, DEFEND, JUMP, INTERACT, DIRECTION
    };



    public enum EventPressType
    {
        PRESSED, RELEASED, HELD
    };

    // TODO: Consider the convention that EventDirection == null means direction is not important, while EventDirection == EventDirection.None means it should be none (no direction pressed)
    // TODO make the directions relative to the player direction on the required sequence

    // Directions in the InputBuffer are relative, but in the InputSequence of a move they are relative to the player's direction

    /// <summary>
    /// The press / release of a button
    /// </summary>
    public struct Event
    {
        public EventType inputType;
        public EventPressType pressType;
        public float holdDuration;

        public float eventTime;
        public EventDirection direction;
        public EventDirection rawDirection;


        public Event(EventType inputType, EventPressType pressType, EventDirection direction = EventDirection.None, float holdDuration = -1, EventDirection rawDirection = EventDirection.None)
        {
            this.inputType = inputType;
            this.pressType = pressType;
            this.holdDuration = holdDuration;
            this.eventTime = Time.time;
            this.direction = direction;
            this.rawDirection = rawDirection;
        }
        public override string ToString() => $"{this.inputType}, {this.pressType}, {this.holdDuration}, {this.eventTime}";


        public override bool Equals(object obj)
        {
            if (!(obj is Event))
                return false;

            Event otherEvent = (Event)obj;
            bool directionsAgree = Shared.Directions.DirectionsAgree(this.direction, otherEvent.direction);
            if (this.inputType == otherEvent.inputType && this.pressType == otherEvent.pressType && directionsAgree) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true when (this) input event satisfies a required event (important for the direction requirement).
        /// If the required event has a direction, then the input event (this) does not satisfy it if this.EventDirection == EventDirection.None
        /// </summary>
        /// <param name="requiredEvent"></param>
        /// <returns></returns>
        public bool Satisfies(Event requiredEvent)
        {
            if (SameButton(requiredEvent)) {
                if (requiredEvent.direction == this.direction) {
                    return true;
                } else {
                    if (requiredEvent.direction == EventDirection.None) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool SameButton(Event requiredEvent)
        {
            return this.inputType == requiredEvent.inputType && this.pressType == requiredEvent.pressType;
        }

    }

    public class InputBuffer
    {
        // Ordered from oldest to newest
        private List<Event> _eventQueue;
        // How many seconds of events to keep in the input buffer (events older than that will be removed)
        private float inputBufferDuration = 2f;
        private Dictionary<EventType, float> _eventPressTimes; // Keep track of the type each button was PRESSED to know the holdDuration of the respective RELEASED event
        private Dictionary<EventType, Event> _heldEvents; // The buttons that are currently held down
        private List<EventDirection> _heldDirections; // The current directions that are held down. Only the most recent one is used as a heldEvent. When empty it removes the heldEvent.


        public InputBuffer(float inputBufferDuration = 2f)
        {
            _eventQueue = new List<Event>();
            _heldEvents = new Dictionary<EventType, Event>();
            _eventPressTimes = new Dictionary<EventType, float>();
            _heldDirections = new List<EventDirection>();
            this.inputBufferDuration = inputBufferDuration;
        }

        public void RegisterInputEvent(EventType eventType, EventPressType pressType, EventDirection direction = EventDirection.None, EventDirection rawDirection = EventDirection.None)
        {
            if (pressType == EventPressType.PRESSED) {
                _eventPressTimes[eventType] = Time.time;
                _heldEvents[eventType] = new Event(eventType, EventPressType.HELD, direction, -1, rawDirection);

                if (eventType == EventType.DIRECTION)
                    _heldDirections.Add(direction);
            }

            // If it is a released event, find the _duration it was held for based on the time since it was last pressed
            float holdDuration = -1;
            if (pressType == EventPressType.RELEASED && _eventPressTimes.ContainsKey(eventType)) {

                holdDuration = Time.time - _eventPressTimes[eventType];

                if (eventType == EventType.DIRECTION) {
                    //Debug.Log("Direction released: " + direction);

                    _heldDirections.Remove(direction);
                    if (_heldDirections.Count == 0) {
                        _heldEvents.Remove(eventType);
                    } else {
                        Event e = _heldEvents[eventType];
                        e.direction = _heldDirections[_heldDirections.Count - 1];
                        _heldEvents[eventType] = e;
                    }
                } else {
                    _heldEvents.Remove(eventType);
                }
            }

            // HELD events do not get added to the buffer
            if (pressType != EventPressType.HELD) {
                Event inputEvent = new Event(eventType, pressType, direction, holdDuration, rawDirection);
                _eventQueue.Add(inputEvent);
                //Debug.Log(inputEvent.ToString());
            }

            //Debug.Log("=============================" + Time.frameCount);
            //float t = Time.time;
            //foreach (Event e in _eventQueue)
            //    Debug.Log(t + ": " + e.inputType + " " + e.pressType + " " + e.direction + " " + e.eventTime);


        }

        public void ClearAll()
        {
            _eventQueue.Clear();
            _heldEvents.Clear();
            _heldDirections.Clear();
        }

        /// <summary>
        /// Cleanup inputs from the buffer that are older than the INPUT_BUFFER_DURATION
        /// </summary>
        public void Cleanup()
        {
            while (_eventQueue.Count > 0 && (Time.time - _eventQueue[0].eventTime) > this.inputBufferDuration) {
                _eventQueue.RemoveAt(0);
            }
        }

        private bool EventSequenceSatisfied(Move move, EventSequence moveSequence)
        {

            bool unheldEventsSatisfied = moveSequence._unheldEvents.Count == 0;
            bool heldEventsSatisfied = moveSequence._heldEvents.Count == 0;

            if (!unheldEventsSatisfied) {

                List<Event> unheldMoveEvents = moveSequence._unheldEvents;
                // Skip all the sequence events that are older than the inputWindow
                int inputStartIdx = 0;
                while (inputStartIdx < _eventQueue.Count && _eventQueue[inputStartIdx].eventTime < Time.time - move.inputTimeWindow) {
                    inputStartIdx++;
                }
                int inputEndIdx = _eventQueue.Count - unheldMoveEvents.Count;

                // For each possible start event (every time a check fails the next event is considered as the possible start event)
                for (; inputStartIdx <= inputEndIdx; inputStartIdx++) {

                    // Need to document the events that were pressed
                    move.givenInputSequence.Clear();
                    move.givenInputSequenceIdxs.Clear();

                    int satisfiedEvents = 0;
                    int inputIdx = inputStartIdx;
                    int moveIdx = 0;
                    while (moveIdx < unheldMoveEvents.Count) {
                        Event moveEvent = unheldMoveEvents[moveIdx];
                        bool eventCheckFailed = true;

                        // Find the next input in the buffer that satisfied the current input requirement
                        while (inputIdx < _eventQueue.Count) {
                            Event inputtedEvent = _eventQueue[inputIdx];
                            // If the same event and input was given inside the move's input time window, go to the next move requirement
                            bool requirementSatisfied = inputtedEvent.Satisfies(moveEvent);
                            if (requirementSatisfied) {
                                move.givenInputSequence.Add(inputtedEvent);
                                move.givenInputSequenceIdxs.Add(inputIdx);
                                inputIdx++;
                                eventCheckFailed = false;
                                break;
                            } else {
                                // If strict order was broken stop the loop immediately
                                if (move.strictOrder) {
                                    eventCheckFailed = true;
                                    break;
                                } else {
                                    // keep on searching in the input for the next movement event
                                    inputIdx++;
                                }
                            }
                        }
                        // If not a StrictOrder and the move was not satisfied, it can not possibly be satisfied even if we start fom the next input index
                        //if(!move.strictOrder && inputIdx == _eventQueue.Count) {
                        if (eventCheckFailed) {
                            break;
                        } else {
                            satisfiedEvents += 1;
                            if (satisfiedEvents == unheldMoveEvents.Count) {
                                unheldEventsSatisfied = true;
                            }
                        }
                        moveIdx += 1;
                    }

                    if (unheldEventsSatisfied && move.viableInput(move.givenInputSequence)) {
                        break;
                    } else {
                        unheldEventsSatisfied = false;
                    }

                }
            }

            if (!heldEventsSatisfied) {
                foreach (Event heldMoveEvent in moveSequence._heldEvents) {
                    // For held events we just check if the same button is held down
                    if (this._heldEvents.ContainsKey(heldMoveEvent.inputType)) {
                        Event heldInputEvent = this._heldEvents[heldMoveEvent.inputType];
                        if (heldInputEvent.Satisfies(heldMoveEvent)) {
                            heldEventsSatisfied = true;
                            move.givenInputSequence.Add(heldInputEvent);
                        }
                    }
                }
            }

            bool moveSatisfied = heldEventsSatisfied && unheldEventsSatisfied;
            if (!moveSatisfied) {
                move.givenInputSequence.Clear();
                move.givenInputSequenceIdxs.Clear();
            }

            return moveSatisfied;
        }


        /// <summary>
        /// Check if any of the move's input sequences is inside the input buffer
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public bool MoveSatisfied(Move move)
        {
            // Check all of the alternative input sequences
            foreach (EventSequence moveSequence in move.inputSequences) {
                if (EventSequenceSatisfied(move, moveSequence)) {
                    return true;
                }
            }
            return false;
        }


        public void ClearPerformedEvents(List<int> eventIdxs)
        {
            // Remove the input events that triggered the selected move
            GenericUtils.RemoveByIndexes(_eventQueue, eventIdxs);
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < _eventQueue.Count; i++) {
                //s += eventQueue[i] + "\n";
                s += _eventQueue[i].inputType + "(" + _eventQueue[i].pressType + "," + _eventQueue[i].direction + "), ";
            }
            return s;
        }

    }

    public struct EventSequence
    {
        public List<Event> _eventsList;
        public HashSet<(EventType, EventPressType)> _uniqueEvents;

        public List<Event> _heldEvents;
        public List<Event> _unheldEvents;

        public EventSequence(List<Event> buttonSequence)
        {
            _eventsList = buttonSequence;

            _uniqueEvents = new HashSet<(EventType, EventPressType)>();
            foreach (Event e in _eventsList) {
                _uniqueEvents.Add((e.inputType, e.pressType));
            }

            _heldEvents = new List<Event>();
            _unheldEvents = new List<Event>();
            foreach (Event e in _eventsList) {
                if (e.pressType == EventPressType.HELD) {
                    _heldEvents.Add(e);
                } else {
                    _unheldEvents.Add(e);
                }
            }
        }
    }

    public struct Move
    {
        // public Properties because they are used by MoveList
        public string name;
        public int priority; //Smaller means higher priority
        public float inputTimeWindow; // How long ago it is allowed for the move's input to have been given
        // List of all the alternative input sequences. Directions are relative to the player, but in the InputBuffer, they are absolute
        public List<EventSequence> inputSequences;
        public System.Func<List<Event>, bool> canPerform;
        public System.Action<List<Event>> perform;
        public System.Func<List<Event>, bool> viableInput;

        // The exact list of events that executed the move
        public List<Event> givenInputSequence;
        public List<int> givenInputSequenceIdxs;

        // The inputs must be given in the exact sequence without any other random buttons pushed in-between
        public bool strictOrder;
        // Foreach input sequence, what are the unique buttons

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rawInputSequences"></param>
        /// <param name="inputTimeWindow"></param>
        /// <param name="canPerform"></param>
        /// <param name="perform"></param>
        /// <param name="priority">Smaller means higher priority</param>
        /// <param name="viableInput"></param>
        /// <param name="strictOrder">The inputs must be given in the exact sequence without any other random buttons pushed in-between</param>
        public Move(string name,
                    List<List<Event>> rawInputSequences,
                    float inputTimeWindow, System.Func<List<Event>, bool> canPerform,
                    System.Action<List<Event>> perform,
                    int priority,
                    System.Func<List<Event>, bool> viableInput,
                    bool strictOrder = false)
        {
            this.name = name;
            this.inputTimeWindow = inputTimeWindow;
            this.canPerform = canPerform;
            this.perform = perform;
            this.priority = priority;
            this.givenInputSequence = new List<Event>();
            this.givenInputSequenceIdxs = new List<int>();
            this.viableInput = viableInput;
            this.strictOrder = strictOrder;

            this.inputSequences = new List<EventSequence>();
            foreach (List<Event> rawInputSequence in rawInputSequences) {
                inputSequences.Add(new EventSequence(rawInputSequence));
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="inputSequence"></param>
        /// <param name="inputTimeWindow"></param>
        /// <param name="canPerform"></param>
        /// <param name="perform"></param>
        /// <param name="priority">Smaller means higher priority</param>

        public Move(string name, List<List<Event>> inputSequence, float inputTimeWindow, System.Func<List<Event>, bool> canPerform, System.Action<List<Event>> perform, int priority)
            : this(name, inputSequence, inputTimeWindow, canPerform, perform, priority, _ => true) { }

    }

    public class MoveList
    {
        //Ordered from high priority to low priority
        private List<Move> moves;

        public MoveList()
        {
            moves = new List<Move>();
        }

        /// <summary>
        /// Insert the action to the MoveList in the correct position according to its priority
        /// </summary>
        /// <param name="move"></param>
        public void AddMove(Move move)
        {
            int i;
            for (i = 0; i < moves.Count; i++) {
                if (move.priority <= moves[i].priority) {
                    break;
                }
            }
            moves.Insert(i, move);
        }

        /// <summary>
        /// Process the input buffer to find and execute the move with the highest priority that was inputted 
        /// </summary>
        /// <param name="inputBuffer"></param>
        /// <returns></returns>
        public Move? ExecuteMoves(InputBuffer inputBuffer)
        {
            Move? executedMove = null;
            for (int i = 0; i < moves.Count; i++) {
                Move move = moves[i];
                if (inputBuffer.MoveSatisfied(move) && move.canPerform(move.givenInputSequence)) {
                    executedMove = move;
                    move.perform(move.givenInputSequence);
                    inputBuffer.ClearPerformedEvents(move.givenInputSequenceIdxs);
                    break;
                }
            }
            return executedMove;
        }


    }

}