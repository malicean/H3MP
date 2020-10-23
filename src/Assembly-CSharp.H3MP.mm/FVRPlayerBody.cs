using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FistVR
{
    public class patch_FVRPlayerBody
    {
        public void SetOutfit(SosigEnemyTemplate tem)
		{
			if (this.m_sosigPlayerBody == null)
			{
				return;
			}

			;
			SosigEnemyID mbclothing = GM.Options.ControlOptions.MBClothing = tem.SosigEnemyID;

			if (mbclothing != SosigEnemyID.None)
			{
				SosigEnemyTemplate sosigEnemyTemplate = ManagerSingleton<IM>.Instance.odicSosigObjsByID[mbclothing];
				if (sosigEnemyTemplate.OutfitConfig.Count > 0)
				{
					SosigOutfitConfig o = sosigEnemyTemplate.OutfitConfig[UnityEngine.Random.Range(0, sosigEnemyTemplate.OutfitConfig.Count)];
					this.m_sosigPlayerBody.ApplyOutfit(o);
				}
			}
		}

		// Token: 0x0600157A RID: 5498 RVA: 0x00097F14 File Offset: 0x00096314
		public void UpdateSosigPlayerBodyState()
		{
			if (GM.Options.ControlOptions.MBMode == ControlOptions.MeatBody.Enabled)
			{
				if (this.m_sosigPlayerBody == null)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.PlayerSosigBodyPrefab.GetGameObject(), GM.CurrentPlayerBody.Torso.position, GM.CurrentPlayerBody.Torso.rotation);
					this.m_sosigPlayerBody = gameObject.GetComponent<PlayerSosigBody>();
					SosigEnemyID mbclothing = GM.Options.ControlOptions.MBClothing;
					Debug.Log("Setting to:" + mbclothing);
					if (mbclothing != SosigEnemyID.None)
					{
						SosigEnemyTemplate sosigEnemyTemplate = ManagerSingleton<IM>.Instance.odicSosigObjsByID[mbclothing];
						if (sosigEnemyTemplate.OutfitConfig.Count > 0)
						{
							SosigOutfitConfig o = sosigEnemyTemplate.OutfitConfig[UnityEngine.Random.Range(0, sosigEnemyTemplate.OutfitConfig.Count)];
							this.m_sosigPlayerBody.ApplyOutfit(o);
						}
					}
				}
			}
			else if (this.m_sosigPlayerBody != null)
			{
				UnityEngine.Object.Destroy(this.m_sosigPlayerBody.gameObject);
				this.m_sosigPlayerBody = null;
			}
		}
    }
}
