using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommanTickManager
{

    public class ProcessingUpdate : MonoBehaviour
    {
        private List<ITick> ticks = new List<ITick>(25);
        private List<IFixedTick> fixedTicks = new List<IFixedTick>();
        private List<ILateTick> lateTicks = new List<ILateTick>();
        private static ProcessingUpdate processingUpdate;
        private int countTicks;
        private int countTicksFixed;
        private int countTicksLate;
        public static ProcessingUpdate Instance
        {
            get
            {
                return processingUpdate;
            }
        }
        void Awake()
        {
            if (processingUpdate == null)
            {
                processingUpdate = this;
            }
            processingUpdate = this;
        }
        public void Add(object updateble)
        {
            var tickable = updateble as ITick;
            if (tickable != null)
            {
                if (!ticks.Contains(tickable))
                {
                    ticks.Add(tickable);
                    countTicks++;
                }
            }

            var tickableFixed = updateble as IFixedTick;
            if (tickableFixed != null)
            {
                if (!fixedTicks.Contains(tickableFixed))
                {
                    fixedTicks.Add(tickableFixed);
                    countTicksFixed++;
                }
            }

            var tickableLate = updateble as ILateTick;
            if (tickableLate != null)
            {
                if (!lateTicks.Contains(tickableLate))
                {
                    lateTicks.Add(tickableLate);
                    countTicksLate++;
                }
            }
        }
        public void Remove(object updateble)
        {
            var tickable = updateble as ITick;
            if (tickable != null)
            {
                if (ticks.Contains(tickable))
                {
                    ticks.Remove(tickable);
                    countTicks--;
                }
            }

            var fixedTickable = updateble as IFixedTick;
            if (fixedTickable != null)
            {
                if (fixedTicks.Contains(fixedTickable))
                {
                    fixedTicks.Remove(fixedTickable);
                    countTicksFixed--;
                }
            }

            var lateTickable = updateble as ILateTick;
            if (lateTickable != null)
            {
                if (lateTicks.Contains(lateTickable))
                {
                    lateTicks.Remove(lateTickable);
                    countTicksLate--;
                }
            }

        }
        private void Update()
        {
            for (var i = 0; i < countTicks; i++)
            {
                ticks[i].Tick();
            }
        }
        private void FixedUpdate()
        {
            for (var i = 0; i < countTicksFixed; i++)
            {
                fixedTicks[i].FixedTick();
            }
        }
        private void LateUpdate()
        {
            for (var i = 0; i < countTicksLate; i++)
            {
                lateTicks[i].LateTick();
            }
        }
    }
}