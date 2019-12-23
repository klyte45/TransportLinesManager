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
        private readonly Action<UIComponent> m_suspendLayout = (Action<UIComponent>)ReflectionUtils.GetMethodDelegate("SuspendLayout", typeof(UIComponent), typeof(Action<UIComponent>));
        private readonly Action<UIComponent> m_resumeLayout = (Action<UIComponent>)ReflectionUtils.GetMethodDelegate("ResumeLayout", typeof(UIComponent), typeof(Action<UIComponent>));


        [SerializeField]
        protected bool m_AutoFitChildrenHorizontally;

        // Token: 0x04000600 RID: 1536
        [SerializeField]
        protected bool m_AutoFitChildrenVertically;

        public bool AutoFitChildrenHorizontally
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
        public bool AutoFitChildrenVertically
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
                m_suspendLayout(this);
                this.Invalidate();
            }
            finally
            {
                m_resumeLayout(this);
            }
        }

        public override void Update()
        {
            if (m_IsComponentInvalidated && (AutoFitChildrenHorizontally || AutoFitChildrenVertically) && isVisible)
            {
                try
                {
                    m_suspendLayout(this);
                    if (AutoFitChildrenHorizontally) FitChildrenHorizontally();
                    if (AutoFitChildrenVertically) FitChildrenVertically();
                }
                finally
                {
                    m_resumeLayout(this);
                }
            }
            base.Update();
        }
    }

}


