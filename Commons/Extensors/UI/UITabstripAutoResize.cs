using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ICities;
using ColossalFramework.UI;
using ColossalFramework;
using ColossalFramework.Plugins;
using System.Threading;
using System;
using System.Linq;
using Klyte.Commons.Utils;

namespace Klyte.Commons.Extensors
{
    public class UITabstripAutoResize : UITabstrip
    {
        private readonly Action<UIComponent> SuspendLayout = (Action<UIComponent>)ReflectionUtils.GetMethodDelegate("SuspendLayout", typeof(UIComponent), typeof(Action<UIComponent>));
        private readonly Action<UIComponent> ResumeLayout = (Action<UIComponent>)ReflectionUtils.GetMethodDelegate("ResumeLayout", typeof(UIComponent), typeof(Action<UIComponent>));


        [SerializeField]
        protected bool m_AutoFitChildrenHorizontally;

        // Token: 0x04000600 RID: 1536
        [SerializeField]
        protected bool m_AutoFitChildrenVertically;

        public bool autoFitChildrenHorizontally
        {
            get {
                return this.m_AutoFitChildrenHorizontally;
            }
            set {
                if (value != this.m_AutoFitChildrenHorizontally)
                {
                    this.m_AutoFitChildrenHorizontally = value;
                    this.Reset();
                }
            }
        }
        public bool autoFitChildrenVertically
        {
            get {
                return this.m_AutoFitChildrenVertically;
            }
            set {
                if (value != this.m_AutoFitChildrenVertically)
                {
                    this.m_AutoFitChildrenVertically = value;
                    this.Reset();
                }
            }
        }

        public void Reset()
        {
            try
            {
                SuspendLayout(this);
                this.Invalidate();
            }
            finally
            {
                ResumeLayout(this);
            }
        }

        public override void Update()
        {
            if (m_IsComponentInvalidated && (autoFitChildrenHorizontally || autoFitChildrenVertically) && isVisible)
            {
                try
                {
                    SuspendLayout(this);
                    if (autoFitChildrenHorizontally) FitChildrenHorizontally();
                    if (autoFitChildrenVertically) FitChildrenVertically();
                }
                finally
                {
                    ResumeLayout(this);
                }
            }
            base.Update();
        }
    }

}


