using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultrabit
{
    public enum eState
    {
        INIT,
        INIT_NO_FILE,
        INIT_WITH_FILE,
        INIT_NO_MICROBIT,
        MICROBIT_CONNECTED,
        COPY_IN_PROGRESS,
        COPY_COMPLETED
    }

    class StateMachine
    {
        private eState _state = eState.INIT;

        public delegate void StateChangeHandler(eState oldState, eState newState);
        public event StateChangeHandler OnStateChange;

        public eState State
        {
            get { return _state; }
            set
            {
                eState oldState = _state;
                _state = value;
                OnStateChange?.Invoke(oldState, value);
            }
        }

    }
}
