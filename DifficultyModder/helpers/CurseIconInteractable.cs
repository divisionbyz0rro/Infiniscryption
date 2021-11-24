using System;
using UnityEngine;
using DiskCardGame;
using System.Collections.Generic;

namespace Infiniscryption.Curses.Helpers
{
	// Token: 0x020003BD RID: 957
	public class CurseIconInteractable : AlternateInputInteractable
	{
		public bool CurseAssigned
		{
			get
			{
				return this.Curse != null;
			}
		}

		public override CursorType CursorType
		{
			get
			{
				return CursorType.Inspect;
			}
		}

		public CurseBase Curse { get; private set; }

		public void AssignCurse(CurseBase curse)
		{
			this.Curse = curse;
			if (curse == null)
			{
				this.gameObject.SetActive(false);
				return;
			}
			this.gameObject.SetActive(true);
			this.GetComponent<Renderer>().material.mainTexture = curse.IconTexture;
		}

		protected override void OnAlternateSelectStarted()
		{
			Singleton<RuleBookController>.Instance.OpenToBoonPage(Curse.ID);
		}
	}
}
