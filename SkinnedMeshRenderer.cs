using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using UnityEngineInternal;

namespace UnityEngine
{
	// Token: 0x02000188 RID: 392
	[RequireComponent(typeof(Transform))]
	[NativeHeader("Runtime/Graphics/GraphicsScriptBindings.h")]
	[NativeHeader("Runtime/Graphics/Renderer.h")]
	[UsedByNativeCode]
	public class Renderer : Component
	{
		// Token: 0x170002D1 RID: 721
		// (get) Token: 0x06000FFE RID: 4094 RVA: 0x0001C544 File Offset: 0x0001A744
		// (set) Token: 0x06000FFF RID: 4095 RVA: 0x0001C55F File Offset: 0x0001A75F
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use shadowCastingMode instead.", false)]
		public bool castShadows
		{
			get
			{
				return this.shadowCastingMode > ShadowCastingMode.Off;
			}
			set
			{
				this.shadowCastingMode = (value ? ShadowCastingMode.On : ShadowCastingMode.Off);
			}
		}

		// Token: 0x170002D2 RID: 722
		// (get) Token: 0x06001000 RID: 4096 RVA: 0x0001C570 File Offset: 0x0001A770
		// (set) Token: 0x06001001 RID: 4097 RVA: 0x0001C58B File Offset: 0x0001A78B
		[Obsolete("Use motionVectorGenerationMode instead.", false)]
		public bool motionVectors
		{
			get
			{
				return this.motionVectorGenerationMode == MotionVectorGenerationMode.Object;
			}
			set
			{
				this.motionVectorGenerationMode = (value ? MotionVectorGenerationMode.Object : MotionVectorGenerationMode.Camera);
			}
		}

		// Token: 0x170002D3 RID: 723
		// (get) Token: 0x06001002 RID: 4098 RVA: 0x0001C59C File Offset: 0x0001A79C
		// (set) Token: 0x06001003 RID: 4099 RVA: 0x0001C5B7 File Offset: 0x0001A7B7
		[Obsolete("Use lightProbeUsage instead.", false)]
		public bool useLightProbes
		{
			get
			{
				return this.lightProbeUsage > LightProbeUsage.Off;
			}
			set
			{
				this.lightProbeUsage = (value ? LightProbeUsage.BlendProbes : LightProbeUsage.Off);
			}
		}

		// Token: 0x170002D4 RID: 724
		// (get) Token: 0x06001004 RID: 4100 RVA: 0x0001C5C8 File Offset: 0x0001A7C8
		// (set) Token: 0x06001005 RID: 4101 RVA: 0x0001C5F0 File Offset: 0x0001A7F0
		public Bounds bounds
		{
			[FreeFunction(Name = "RendererScripting::GetWorldBounds", HasExplicitThis = true)]
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Bounds result;
				Renderer.get_bounds_Injected(intPtr, out result);
				return result;
			}
			[NativeName("SetWorldAABB")]
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_bounds_Injected(intPtr, ref value);
			}
		}

		// Token: 0x170002D5 RID: 725
		// (get) Token: 0x06001006 RID: 4102 RVA: 0x0001C614 File Offset: 0x0001A814
		// (set) Token: 0x06001007 RID: 4103 RVA: 0x0001C63C File Offset: 0x0001A83C
		public Bounds localBounds
		{
			[FreeFunction(Name = "RendererScripting::GetLocalBounds", HasExplicitThis = true)]
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Bounds result;
				Renderer.get_localBounds_Injected(intPtr, out result);
				return result;
			}
			[NativeName("SetLocalAABB")]
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_localBounds_Injected(intPtr, ref value);
			}
		}

		// Token: 0x06001008 RID: 4104 RVA: 0x0001C660 File Offset: 0x0001A860
		[NativeName("ResetWorldAABB")]
		public void ResetBounds()
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.ResetBounds_Injected(intPtr);
		}

		// Token: 0x06001009 RID: 4105 RVA: 0x0001C684 File Offset: 0x0001A884
		[NativeName("ResetLocalAABB")]
		public void ResetLocalBounds()
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.ResetLocalBounds_Injected(intPtr);
		}

		// Token: 0x0600100A RID: 4106 RVA: 0x0001C6A8 File Offset: 0x0001A8A8
		[FreeFunction(Name = "RendererScripting::SetStaticLightmapST", HasExplicitThis = true)]
		private void SetStaticLightmapST(Vector4 st)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.SetStaticLightmapST_Injected(intPtr, ref st);
		}

		// Token: 0x0600100B RID: 4107 RVA: 0x0001C6CC File Offset: 0x0001A8CC
		[FreeFunction(Name = "RendererScripting::GetMaterial", HasExplicitThis = true)]
		private Material GetMaterial()
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			return Unmarshal.UnmarshalUnityObject<Material>(Renderer.GetMaterial_Injected(intPtr));
		}

		// Token: 0x0600100C RID: 4108 RVA: 0x0001C6F4 File Offset: 0x0001A8F4
		[FreeFunction(Name = "RendererScripting::GetSharedMaterial", HasExplicitThis = true)]
		private Material GetSharedMaterial()
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			return Unmarshal.UnmarshalUnityObject<Material>(Renderer.GetSharedMaterial_Injected(intPtr));
		}

		// Token: 0x0600100D RID: 4109 RVA: 0x0001C71C File Offset: 0x0001A91C
		[FreeFunction(Name = "RendererScripting::SetMaterial", HasExplicitThis = true)]
		private void SetMaterial(Material m)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.SetMaterial_Injected(intPtr, Object.MarshalledUnityObject.Marshal<Material>(m));
		}

		// Token: 0x0600100E RID: 4110 RVA: 0x0001C744 File Offset: 0x0001A944
		[FreeFunction(Name = "RendererScripting::GetMaterialArray", HasExplicitThis = true)]
		private Material[] GetMaterialArray()
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			return Renderer.GetMaterialArray_Injected(intPtr);
		}

		// Token: 0x0600100F RID: 4111 RVA: 0x0001C768 File Offset: 0x0001A968
		[FreeFunction(Name = "RendererScripting::GetMaterialArray", HasExplicitThis = true)]
		private void CopyMaterialArray([Out] Material[] m)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.CopyMaterialArray_Injected(intPtr, m);
		}

		// Token: 0x06001010 RID: 4112 RVA: 0x0001C78C File Offset: 0x0001A98C
		[FreeFunction(Name = "RendererScripting::GetSharedMaterialArray", HasExplicitThis = true)]
		private void CopySharedMaterialArray([Out] Material[] m)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.CopySharedMaterialArray_Injected(intPtr, m);
		}

		// Token: 0x06001011 RID: 4113 RVA: 0x0001C7B0 File Offset: 0x0001A9B0
		[FreeFunction(Name = "RendererScripting::SetMaterialArray", HasExplicitThis = true)]
		private void SetMaterialArray([NotNull] Material[] m, int length)
		{
			if (m == null)
			{
				ThrowHelper.ThrowArgumentNullException(m, "m");
			}
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.SetMaterialArray_Injected(intPtr, m, length);
		}

		// Token: 0x06001012 RID: 4114 RVA: 0x0001C7E3 File Offset: 0x0001A9E3
		private void SetMaterialArray(Material[] m)
		{
			this.SetMaterialArray(m, (m != null) ? m.Length : 0);
		}

		// Token: 0x06001013 RID: 4115 RVA: 0x0001C7F8 File Offset: 0x0001A9F8
		[FreeFunction(Name = "RendererScripting::SetPropertyBlock", HasExplicitThis = true)]
		internal void Internal_SetPropertyBlock(MaterialPropertyBlock properties)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.Internal_SetPropertyBlock_Injected(intPtr, (properties == null) ? ((IntPtr)0) : MaterialPropertyBlock.BindingsMarshaller.ConvertToNative(properties));
		}

		// Token: 0x06001014 RID: 4116 RVA: 0x0001C82C File Offset: 0x0001AA2C
		[FreeFunction(Name = "RendererScripting::GetPropertyBlock", HasExplicitThis = true)]
		internal void Internal_GetPropertyBlock([NotNull] MaterialPropertyBlock dest)
		{
			if (dest == null)
			{
				ThrowHelper.ThrowArgumentNullException(dest, "dest");
			}
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			IntPtr intPtr2 = MaterialPropertyBlock.BindingsMarshaller.ConvertToNative(dest);
			if (intPtr2 == 0)
			{
				ThrowHelper.ThrowArgumentNullException(dest, "dest");
			}
			Renderer.Internal_GetPropertyBlock_Injected(intPtr, intPtr2);
		}

		// Token: 0x06001015 RID: 4117 RVA: 0x0001C874 File Offset: 0x0001AA74
		[FreeFunction(Name = "RendererScripting::SetPropertyBlockMaterialIndex", HasExplicitThis = true)]
		internal void Internal_SetPropertyBlockMaterialIndex(MaterialPropertyBlock properties, int materialIndex)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.Internal_SetPropertyBlockMaterialIndex_Injected(intPtr, (properties == null) ? ((IntPtr)0) : MaterialPropertyBlock.BindingsMarshaller.ConvertToNative(properties), materialIndex);
		}

		// Token: 0x06001016 RID: 4118 RVA: 0x0001C8A8 File Offset: 0x0001AAA8
		[FreeFunction(Name = "RendererScripting::GetPropertyBlockMaterialIndex", HasExplicitThis = true)]
		internal void Internal_GetPropertyBlockMaterialIndex([NotNull] MaterialPropertyBlock dest, int materialIndex)
		{
			if (dest == null)
			{
				ThrowHelper.ThrowArgumentNullException(dest, "dest");
			}
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			IntPtr intPtr2 = MaterialPropertyBlock.BindingsMarshaller.ConvertToNative(dest);
			if (intPtr2 == 0)
			{
				ThrowHelper.ThrowArgumentNullException(dest, "dest");
			}
			Renderer.Internal_GetPropertyBlockMaterialIndex_Injected(intPtr, intPtr2, materialIndex);
		}

		// Token: 0x06001017 RID: 4119 RVA: 0x0001C8F0 File Offset: 0x0001AAF0
		[FreeFunction(Name = "RendererScripting::HasPropertyBlock", HasExplicitThis = true)]
		public bool HasPropertyBlock()
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			return Renderer.HasPropertyBlock_Injected(intPtr);
		}

		// Token: 0x06001018 RID: 4120 RVA: 0x0001C912 File Offset: 0x0001AB12
		public void SetPropertyBlock(MaterialPropertyBlock properties)
		{
			this.Internal_SetPropertyBlock(properties);
		}

		// Token: 0x06001019 RID: 4121 RVA: 0x0001C91D File Offset: 0x0001AB1D
		public void SetPropertyBlock(MaterialPropertyBlock properties, int materialIndex)
		{
			this.Internal_SetPropertyBlockMaterialIndex(properties, materialIndex);
		}

		// Token: 0x0600101A RID: 4122 RVA: 0x0001C929 File Offset: 0x0001AB29
		public void GetPropertyBlock(MaterialPropertyBlock properties)
		{
			this.Internal_GetPropertyBlock(properties);
		}

		// Token: 0x0600101B RID: 4123 RVA: 0x0001C934 File Offset: 0x0001AB34
		public void GetPropertyBlock(MaterialPropertyBlock properties, int materialIndex)
		{
			this.Internal_GetPropertyBlockMaterialIndex(properties, materialIndex);
		}

		// Token: 0x0600101C RID: 4124 RVA: 0x0001C940 File Offset: 0x0001AB40
		[FreeFunction(Name = "RendererScripting::GetClosestReflectionProbes", HasExplicitThis = true)]
		private void GetClosestReflectionProbesInternal(object result)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.GetClosestReflectionProbesInternal_Injected(intPtr, result);
		}

		// Token: 0x170002D6 RID: 726
		// (get) Token: 0x0600101D RID: 4125 RVA: 0x0001C964 File Offset: 0x0001AB64
		// (set) Token: 0x0600101E RID: 4126 RVA: 0x0001C988 File Offset: 0x0001AB88
		public bool enabled
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_enabled_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_enabled_Injected(intPtr, value);
			}
		}

		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x0600101F RID: 4127 RVA: 0x0001C9AC File Offset: 0x0001ABAC
		public bool isVisible
		{
			[NativeName("IsVisibleInScene")]
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_isVisible_Injected(intPtr);
			}
		}

		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x06001020 RID: 4128 RVA: 0x0001C9D0 File Offset: 0x0001ABD0
		// (set) Token: 0x06001021 RID: 4129 RVA: 0x0001C9F4 File Offset: 0x0001ABF4
		public ShadowCastingMode shadowCastingMode
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_shadowCastingMode_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_shadowCastingMode_Injected(intPtr, value);
			}
		}

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x06001022 RID: 4130 RVA: 0x0001CA18 File Offset: 0x0001AC18
		// (set) Token: 0x06001023 RID: 4131 RVA: 0x0001CA3C File Offset: 0x0001AC3C
		public bool receiveShadows
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_receiveShadows_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_receiveShadows_Injected(intPtr, value);
			}
		}

		// Token: 0x170002DA RID: 730
		// (get) Token: 0x06001024 RID: 4132 RVA: 0x0001CA60 File Offset: 0x0001AC60
		// (set) Token: 0x06001025 RID: 4133 RVA: 0x0001CA84 File Offset: 0x0001AC84
		public bool forceRenderingOff
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_forceRenderingOff_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_forceRenderingOff_Injected(intPtr, value);
			}
		}

		// Token: 0x170002DB RID: 731
		// (get) Token: 0x06001026 RID: 4134 RVA: 0x0001CAA8 File Offset: 0x0001ACA8
		// (set) Token: 0x06001027 RID: 4135 RVA: 0x0001CACC File Offset: 0x0001ACCC
		internal bool allowGPUDrivenRendering
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_allowGPUDrivenRendering_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_allowGPUDrivenRendering_Injected(intPtr, value);
			}
		}

		// Token: 0x170002DC RID: 732
		// (get) Token: 0x06001028 RID: 4136 RVA: 0x0001CAF0 File Offset: 0x0001ACF0
		// (set) Token: 0x06001029 RID: 4137 RVA: 0x0001CB14 File Offset: 0x0001AD14
		internal bool smallMeshCulling
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_smallMeshCulling_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_smallMeshCulling_Injected(intPtr, value);
			}
		}

		// Token: 0x0600102A RID: 4138 RVA: 0x0001CB38 File Offset: 0x0001AD38
		[NativeName("GetIsStaticShadowCaster")]
		private bool GetIsStaticShadowCaster()
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			return Renderer.GetIsStaticShadowCaster_Injected(intPtr);
		}

		// Token: 0x0600102B RID: 4139 RVA: 0x0001CB5C File Offset: 0x0001AD5C
		[NativeName("SetIsStaticShadowCaster")]
		private void SetIsStaticShadowCaster(bool value)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.SetIsStaticShadowCaster_Injected(intPtr, value);
		}

		// Token: 0x170002DD RID: 733
		// (get) Token: 0x0600102C RID: 4140 RVA: 0x0001CB80 File Offset: 0x0001AD80
		// (set) Token: 0x0600102D RID: 4141 RVA: 0x0001CB98 File Offset: 0x0001AD98
		public bool staticShadowCaster
		{
			get
			{
				return this.GetIsStaticShadowCaster();
			}
			set
			{
				this.SetIsStaticShadowCaster(value);
			}
		}

		// Token: 0x170002DE RID: 734
		// (get) Token: 0x0600102E RID: 4142 RVA: 0x0001CBA4 File Offset: 0x0001ADA4
		// (set) Token: 0x0600102F RID: 4143 RVA: 0x0001CBC8 File Offset: 0x0001ADC8
		public MotionVectorGenerationMode motionVectorGenerationMode
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_motionVectorGenerationMode_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_motionVectorGenerationMode_Injected(intPtr, value);
			}
		}

		// Token: 0x170002DF RID: 735
		// (get) Token: 0x06001030 RID: 4144 RVA: 0x0001CBEC File Offset: 0x0001ADEC
		// (set) Token: 0x06001031 RID: 4145 RVA: 0x0001CC10 File Offset: 0x0001AE10
		public LightProbeUsage lightProbeUsage
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_lightProbeUsage_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_lightProbeUsage_Injected(intPtr, value);
			}
		}

		// Token: 0x170002E0 RID: 736
		// (get) Token: 0x06001032 RID: 4146 RVA: 0x0001CC34 File Offset: 0x0001AE34
		// (set) Token: 0x06001033 RID: 4147 RVA: 0x0001CC58 File Offset: 0x0001AE58
		public ReflectionProbeUsage reflectionProbeUsage
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_reflectionProbeUsage_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_reflectionProbeUsage_Injected(intPtr, value);
			}
		}

		// Token: 0x170002E1 RID: 737
		// (get) Token: 0x06001034 RID: 4148 RVA: 0x0001CC7C File Offset: 0x0001AE7C
		// (set) Token: 0x06001035 RID: 4149 RVA: 0x0001CCA0 File Offset: 0x0001AEA0
		public uint renderingLayerMask
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_renderingLayerMask_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_renderingLayerMask_Injected(intPtr, value);
			}
		}

		// Token: 0x170002E2 RID: 738
		// (get) Token: 0x06001036 RID: 4150 RVA: 0x0001CCC4 File Offset: 0x0001AEC4
		// (set) Token: 0x06001037 RID: 4151 RVA: 0x0001CCE8 File Offset: 0x0001AEE8
		public int rendererPriority
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_rendererPriority_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_rendererPriority_Injected(intPtr, value);
			}
		}

		// Token: 0x170002E3 RID: 739
		// (get) Token: 0x06001038 RID: 4152 RVA: 0x0001CD0C File Offset: 0x0001AF0C
		// (set) Token: 0x06001039 RID: 4153 RVA: 0x0001CD30 File Offset: 0x0001AF30
		public RayTracingMode rayTracingMode
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_rayTracingMode_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_rayTracingMode_Injected(intPtr, value);
			}
		}

		// Token: 0x170002E4 RID: 740
		// (get) Token: 0x0600103A RID: 4154 RVA: 0x0001CD54 File Offset: 0x0001AF54
		// (set) Token: 0x0600103B RID: 4155 RVA: 0x0001CD78 File Offset: 0x0001AF78
		public RayTracingAccelerationStructureBuildFlags rayTracingAccelerationStructureBuildFlags
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_rayTracingAccelerationStructureBuildFlags_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_rayTracingAccelerationStructureBuildFlags_Injected(intPtr, value);
			}
		}

		// Token: 0x170002E5 RID: 741
		// (get) Token: 0x0600103C RID: 4156 RVA: 0x0001CD9C File Offset: 0x0001AF9C
		// (set) Token: 0x0600103D RID: 4157 RVA: 0x0001CDC0 File Offset: 0x0001AFC0
		public bool rayTracingAccelerationStructureBuildFlagsOverride
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_rayTracingAccelerationStructureBuildFlagsOverride_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_rayTracingAccelerationStructureBuildFlagsOverride_Injected(intPtr, value);
			}
		}

		// Token: 0x170002E6 RID: 742
		// (get) Token: 0x0600103E RID: 4158 RVA: 0x0001CDE4 File Offset: 0x0001AFE4
		// (set) Token: 0x0600103F RID: 4159 RVA: 0x0001CE24 File Offset: 0x0001B024
		public unsafe string sortingLayerName
		{
			get
			{
				string stringAndDispose;
				try
				{
					IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
					if (intPtr == 0)
					{
						ThrowHelper.ThrowNullReferenceException(this);
					}
					ManagedSpanWrapper managedSpan;
					Renderer.get_sortingLayerName_Injected(intPtr, out managedSpan);
				}
				finally
				{
					ManagedSpanWrapper managedSpan;
					stringAndDispose = OutStringMarshaller.GetStringAndDispose(managedSpan);
				}
				return stringAndDispose;
			}
			set
			{
				try
				{
					IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
					if (intPtr == 0)
					{
						ThrowHelper.ThrowNullReferenceException(this);
					}
					ManagedSpanWrapper managedSpanWrapper;
					if (!StringMarshaller.TryMarshalEmptyOrNullString(value, ref managedSpanWrapper))
					{
						ReadOnlySpan<char> readOnlySpan = value.AsSpan();
						fixed (char* ptr = readOnlySpan.GetPinnableReference())
						{
							managedSpanWrapper = new ManagedSpanWrapper((void*)ptr, readOnlySpan.Length);
						}
					}
					Renderer.set_sortingLayerName_Injected(intPtr, ref managedSpanWrapper);
				}
				finally
				{
					char* ptr = null;
				}
			}
		}

		// Token: 0x170002E7 RID: 743
		// (get) Token: 0x06001040 RID: 4160 RVA: 0x0001CE88 File Offset: 0x0001B088
		// (set) Token: 0x06001041 RID: 4161 RVA: 0x0001CEAC File Offset: 0x0001B0AC
		public int sortingLayerID
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_sortingLayerID_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_sortingLayerID_Injected(intPtr, value);
			}
		}

		// Token: 0x170002E8 RID: 744
		// (get) Token: 0x06001042 RID: 4162 RVA: 0x0001CED0 File Offset: 0x0001B0D0
		// (set) Token: 0x06001043 RID: 4163 RVA: 0x0001CEF4 File Offset: 0x0001B0F4
		public int sortingOrder
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_sortingOrder_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_sortingOrder_Injected(intPtr, value);
			}
		}

		// Token: 0x170002E9 RID: 745
		// (get) Token: 0x06001044 RID: 4164 RVA: 0x0001CF18 File Offset: 0x0001B118
		internal uint sortingKey
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_sortingKey_Injected(intPtr);
			}
		}

		// Token: 0x170002EA RID: 746
		// (get) Token: 0x06001045 RID: 4165 RVA: 0x0001CF3C File Offset: 0x0001B13C
		// (set) Token: 0x06001046 RID: 4166 RVA: 0x0001CF60 File Offset: 0x0001B160
		internal int sortingGroupID
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_sortingGroupID_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_sortingGroupID_Injected(intPtr, value);
			}
		}

		// Token: 0x170002EB RID: 747
		// (get) Token: 0x06001047 RID: 4167 RVA: 0x0001CF84 File Offset: 0x0001B184
		// (set) Token: 0x06001048 RID: 4168 RVA: 0x0001CFA8 File Offset: 0x0001B1A8
		internal int sortingGroupOrder
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_sortingGroupOrder_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_sortingGroupOrder_Injected(intPtr, value);
			}
		}

		// Token: 0x170002EC RID: 748
		// (get) Token: 0x06001049 RID: 4169 RVA: 0x0001CFCC File Offset: 0x0001B1CC
		internal uint sortingGroupKey
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_sortingGroupKey_Injected(intPtr);
			}
		}

		// Token: 0x170002ED RID: 749
		// (get) Token: 0x0600104A RID: 4170 RVA: 0x0001CFF0 File Offset: 0x0001B1F0
		public bool isLOD0
		{
			[NativeName("IsLOD0")]
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_isLOD0_Injected(intPtr);
			}
		}

		// Token: 0x170002EE RID: 750
		// (get) Token: 0x0600104B RID: 4171 RVA: 0x0001D014 File Offset: 0x0001B214
		// (set) Token: 0x0600104C RID: 4172 RVA: 0x0001D038 File Offset: 0x0001B238
		[NativeProperty("IsDynamicOccludee")]
		public bool allowOcclusionWhenDynamic
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_allowOcclusionWhenDynamic_Injected(intPtr);
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_allowOcclusionWhenDynamic_Injected(intPtr, value);
			}
		}

		// Token: 0x170002EF RID: 751
		// (get) Token: 0x0600104D RID: 4173 RVA: 0x0001D05C File Offset: 0x0001B25C
		// (set) Token: 0x0600104E RID: 4174 RVA: 0x0001D084 File Offset: 0x0001B284
		[NativeProperty("StaticBatchRoot")]
		internal Transform staticBatchRootTransform
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Unmarshal.UnmarshalUnityObject<Transform>(Renderer.get_staticBatchRootTransform_Injected(intPtr));
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_staticBatchRootTransform_Injected(intPtr, Object.MarshalledUnityObject.Marshal<Transform>(value));
			}
		}

		// Token: 0x170002F0 RID: 752
		// (get) Token: 0x0600104F RID: 4175 RVA: 0x0001D0AC File Offset: 0x0001B2AC
		internal int staticBatchIndex
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_staticBatchIndex_Injected(intPtr);
			}
		}

		// Token: 0x06001050 RID: 4176 RVA: 0x0001D0D0 File Offset: 0x0001B2D0
		internal void SetStaticBatchInfo(int firstSubMesh, int subMeshCount)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.SetStaticBatchInfo_Injected(intPtr, firstSubMesh, subMeshCount);
		}

		// Token: 0x170002F1 RID: 753
		// (get) Token: 0x06001051 RID: 4177 RVA: 0x0001D0F4 File Offset: 0x0001B2F4
		public bool isPartOfStaticBatch
		{
			[NativeName("IsPartOfStaticBatch")]
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Renderer.get_isPartOfStaticBatch_Injected(intPtr);
			}
		}

		// Token: 0x170002F2 RID: 754
		// (get) Token: 0x06001052 RID: 4178 RVA: 0x0001D118 File Offset: 0x0001B318
		public Matrix4x4 worldToLocalMatrix
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Matrix4x4 result;
				Renderer.get_worldToLocalMatrix_Injected(intPtr, out result);
				return result;
			}
		}

		// Token: 0x170002F3 RID: 755
		// (get) Token: 0x06001053 RID: 4179 RVA: 0x0001D140 File Offset: 0x0001B340
		public Matrix4x4 localToWorldMatrix
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Matrix4x4 result;
				Renderer.get_localToWorldMatrix_Injected(intPtr, out result);
				return result;
			}
		}

		// Token: 0x170002F4 RID: 756
		// (get) Token: 0x06001054 RID: 4180 RVA: 0x0001D168 File Offset: 0x0001B368
		// (set) Token: 0x06001055 RID: 4181 RVA: 0x0001D190 File Offset: 0x0001B390
		public GameObject lightProbeProxyVolumeOverride
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Unmarshal.UnmarshalUnityObject<GameObject>(Renderer.get_lightProbeProxyVolumeOverride_Injected(intPtr));
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_lightProbeProxyVolumeOverride_Injected(intPtr, Object.MarshalledUnityObject.Marshal<GameObject>(value));
			}
		}

		// Token: 0x170002F5 RID: 757
		// (get) Token: 0x06001056 RID: 4182 RVA: 0x0001D1B8 File Offset: 0x0001B3B8
		// (set) Token: 0x06001057 RID: 4183 RVA: 0x0001D1E0 File Offset: 0x0001B3E0
		public Transform probeAnchor
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Unmarshal.UnmarshalUnityObject<Transform>(Renderer.get_probeAnchor_Injected(intPtr));
			}
			set
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Renderer.set_probeAnchor_Injected(intPtr, Object.MarshalledUnityObject.Marshal<Transform>(value));
			}
		}

		// Token: 0x06001058 RID: 4184 RVA: 0x0001D208 File Offset: 0x0001B408
		[NativeName("GetLightmapIndexInt")]
		private int GetLightmapIndex(LightmapType lt)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			return Renderer.GetLightmapIndex_Injected(intPtr, lt);
		}

		// Token: 0x06001059 RID: 4185 RVA: 0x0001D22C File Offset: 0x0001B42C
		[NativeName("SetLightmapIndexInt")]
		private void SetLightmapIndex(int index, LightmapType lt)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.SetLightmapIndex_Injected(intPtr, index, lt);
		}

		// Token: 0x0600105A RID: 4186 RVA: 0x0001D250 File Offset: 0x0001B450
		[NativeName("GetLightmapST")]
		private Vector4 GetLightmapST(LightmapType lt)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Vector4 result;
			Renderer.GetLightmapST_Injected(intPtr, lt, out result);
			return result;
		}

		// Token: 0x0600105B RID: 4187 RVA: 0x0001D278 File Offset: 0x0001B478
		[NativeName("SetLightmapST")]
		private void SetLightmapST(Vector4 st, LightmapType lt)
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			Renderer.SetLightmapST_Injected(intPtr, ref st, lt);
		}

		// Token: 0x170002F6 RID: 758
		// (get) Token: 0x0600105C RID: 4188 RVA: 0x0001D2A0 File Offset: 0x0001B4A0
		// (set) Token: 0x0600105D RID: 4189 RVA: 0x0001D2B9 File Offset: 0x0001B4B9
		public int lightmapIndex
		{
			get
			{
				return this.GetLightmapIndex(LightmapType.StaticLightmap);
			}
			set
			{
				this.SetLightmapIndex(value, LightmapType.StaticLightmap);
			}
		}

		// Token: 0x170002F7 RID: 759
		// (get) Token: 0x0600105E RID: 4190 RVA: 0x0001D2C8 File Offset: 0x0001B4C8
		// (set) Token: 0x0600105F RID: 4191 RVA: 0x0001D2E1 File Offset: 0x0001B4E1
		public int realtimeLightmapIndex
		{
			get
			{
				return this.GetLightmapIndex(LightmapType.DynamicLightmap);
			}
			set
			{
				this.SetLightmapIndex(value, LightmapType.DynamicLightmap);
			}
		}

		// Token: 0x170002F8 RID: 760
		// (get) Token: 0x06001060 RID: 4192 RVA: 0x0001D2F0 File Offset: 0x0001B4F0
		// (set) Token: 0x06001061 RID: 4193 RVA: 0x0001D309 File Offset: 0x0001B509
		public Vector4 lightmapScaleOffset
		{
			get
			{
				return this.GetLightmapST(LightmapType.StaticLightmap);
			}
			set
			{
				this.SetStaticLightmapST(value);
			}
		}

		// Token: 0x170002F9 RID: 761
		// (get) Token: 0x06001062 RID: 4194 RVA: 0x0001D314 File Offset: 0x0001B514
		// (set) Token: 0x06001063 RID: 4195 RVA: 0x0001D32D File Offset: 0x0001B52D
		public Vector4 realtimeLightmapScaleOffset
		{
			get
			{
				return this.GetLightmapST(LightmapType.DynamicLightmap);
			}
			set
			{
				this.SetLightmapST(value, LightmapType.DynamicLightmap);
			}
		}

		// Token: 0x06001064 RID: 4196 RVA: 0x0001D33C File Offset: 0x0001B53C
		private int GetMaterialCount()
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			return Renderer.GetMaterialCount_Injected(intPtr);
		}

		// Token: 0x06001065 RID: 4197 RVA: 0x0001D360 File Offset: 0x0001B560
		[NativeName("GetMaterialArray")]
		private Material[] GetSharedMaterialArray()
		{
			IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
			if (intPtr == 0)
			{
				ThrowHelper.ThrowNullReferenceException(this);
			}
			return Renderer.GetSharedMaterialArray_Injected(intPtr);
		}

		// Token: 0x170002FA RID: 762
		// (get) Token: 0x06001066 RID: 4198 RVA: 0x0001D384 File Offset: 0x0001B584
		// (set) Token: 0x06001067 RID: 4199 RVA: 0x0001D39C File Offset: 0x0001B59C
		public Material[] materials
		{
			get
			{
				return this.GetMaterialArray();
			}
			set
			{
				this.SetMaterialArray(value);
			}
		}

		// Token: 0x170002FB RID: 763
		// (get) Token: 0x06001068 RID: 4200 RVA: 0x0001D3A8 File Offset: 0x0001B5A8
		// (set) Token: 0x06001069 RID: 4201 RVA: 0x0001D3C0 File Offset: 0x0001B5C0
		public Material material
		{
			get
			{
				return this.GetMaterial();
			}
			set
			{
				this.SetMaterial(value);
			}
		}

		// Token: 0x170002FC RID: 764
		// (get) Token: 0x0600106A RID: 4202 RVA: 0x0001D3CC File Offset: 0x0001B5CC
		// (set) Token: 0x0600106B RID: 4203 RVA: 0x0001D3C0 File Offset: 0x0001B5C0
		public Material sharedMaterial
		{
			get
			{
				return this.GetSharedMaterial();
			}
			set
			{
				this.SetMaterial(value);
			}
		}

		// Token: 0x170002FD RID: 765
		// (get) Token: 0x0600106C RID: 4204 RVA: 0x0001D3E4 File Offset: 0x0001B5E4
		// (set) Token: 0x0600106D RID: 4205 RVA: 0x0001D39C File Offset: 0x0001B59C
		public Material[] sharedMaterials
		{
			get
			{
				return this.GetSharedMaterialArray();
			}
			set
			{
				this.SetMaterialArray(value);
			}
		}

		// Token: 0x0600106E RID: 4206 RVA: 0x0001D3FC File Offset: 0x0001B5FC
		public void GetMaterials(List<Material> m)
		{
			bool flag = m == null;
			if (flag)
			{
				throw new ArgumentNullException("The result material list cannot be null.", "m");
			}
			NoAllocHelpers.EnsureListElemCount<Material>(m, this.GetMaterialCount());
			this.CopyMaterialArray(NoAllocHelpers.ExtractArrayFromList<Material>(m));
		}

		// Token: 0x0600106F RID: 4207 RVA: 0x0001D43C File Offset: 0x0001B63C
		public void SetSharedMaterials(List<Material> materials)
		{
			bool flag = materials == null;
			if (flag)
			{
				throw new ArgumentNullException("The material list to set cannot be null.", "materials");
			}
			this.SetMaterialArray(NoAllocHelpers.ExtractArrayFromList<Material>(materials), materials.Count);
		}

		// Token: 0x06001070 RID: 4208 RVA: 0x0001D478 File Offset: 0x0001B678
		public void SetMaterials(List<Material> materials)
		{
			bool flag = materials == null;
			if (flag)
			{
				throw new ArgumentNullException("The material list to set cannot be null.", "materials");
			}
			this.SetMaterialArray(NoAllocHelpers.ExtractArrayFromList<Material>(materials), materials.Count);
		}

		// Token: 0x06001071 RID: 4209 RVA: 0x0001D4B4 File Offset: 0x0001B6B4
		public void GetSharedMaterials(List<Material> m)
		{
			bool flag = m == null;
			if (flag)
			{
				throw new ArgumentNullException("The result material list cannot be null.", "m");
			}
			NoAllocHelpers.EnsureListElemCount<Material>(m, this.GetMaterialCount());
			this.CopySharedMaterialArray(NoAllocHelpers.ExtractArrayFromList<Material>(m));
		}

		// Token: 0x06001072 RID: 4210 RVA: 0x0001D4F4 File Offset: 0x0001B6F4
		public void GetClosestReflectionProbes(List<ReflectionProbeBlendInfo> result)
		{
			this.GetClosestReflectionProbesInternal(result);
		}

		// Token: 0x170002FE RID: 766
		// (get) Token: 0x06001073 RID: 4211 RVA: 0x0001D500 File Offset: 0x0001B700
		public LODGroup LODGroup
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Renderer>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Unmarshal.UnmarshalUnityObject<LODGroup>(Renderer.get_LODGroup_Injected(intPtr));
			}
		}

		// Token: 0x06001074 RID: 4212 RVA: 0x0001D527 File Offset: 0x0001B727
		public Renderer()
		{
		}

		// Token: 0x06001075 RID: 4213
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void get_bounds_Injected(IntPtr _unity_self, out Bounds ret);

		// Token: 0x06001076 RID: 4214
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_bounds_Injected(IntPtr _unity_self, [In] ref Bounds value);

		// Token: 0x06001077 RID: 4215
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void get_localBounds_Injected(IntPtr _unity_self, out Bounds ret);

		// Token: 0x06001078 RID: 4216
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_localBounds_Injected(IntPtr _unity_self, [In] ref Bounds value);

		// Token: 0x06001079 RID: 4217
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void ResetBounds_Injected(IntPtr _unity_self);

		// Token: 0x0600107A RID: 4218
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void ResetLocalBounds_Injected(IntPtr _unity_self);

		// Token: 0x0600107B RID: 4219
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetStaticLightmapST_Injected(IntPtr _unity_self, [In] ref Vector4 st);

		// Token: 0x0600107C RID: 4220
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr GetMaterial_Injected(IntPtr _unity_self);

		// Token: 0x0600107D RID: 4221
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr GetSharedMaterial_Injected(IntPtr _unity_self);

		// Token: 0x0600107E RID: 4222
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetMaterial_Injected(IntPtr _unity_self, IntPtr m);

		// Token: 0x0600107F RID: 4223
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Material[] GetMaterialArray_Injected(IntPtr _unity_self);

		// Token: 0x06001080 RID: 4224
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void CopyMaterialArray_Injected(IntPtr _unity_self, [Out] Material[] m);

		// Token: 0x06001081 RID: 4225
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void CopySharedMaterialArray_Injected(IntPtr _unity_self, [Out] Material[] m);

		// Token: 0x06001082 RID: 4226
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetMaterialArray_Injected(IntPtr _unity_self, Material[] m, int length);

		// Token: 0x06001083 RID: 4227
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void Internal_SetPropertyBlock_Injected(IntPtr _unity_self, IntPtr properties);

		// Token: 0x06001084 RID: 4228
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void Internal_GetPropertyBlock_Injected(IntPtr _unity_self, IntPtr dest);

		// Token: 0x06001085 RID: 4229
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void Internal_SetPropertyBlockMaterialIndex_Injected(IntPtr _unity_self, IntPtr properties, int materialIndex);

		// Token: 0x06001086 RID: 4230
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void Internal_GetPropertyBlockMaterialIndex_Injected(IntPtr _unity_self, IntPtr dest, int materialIndex);

		// Token: 0x06001087 RID: 4231
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool HasPropertyBlock_Injected(IntPtr _unity_self);

		// Token: 0x06001088 RID: 4232
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetClosestReflectionProbesInternal_Injected(IntPtr _unity_self, object result);

		// Token: 0x06001089 RID: 4233
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_enabled_Injected(IntPtr _unity_self);

		// Token: 0x0600108A RID: 4234
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_enabled_Injected(IntPtr _unity_self, bool value);

		// Token: 0x0600108B RID: 4235
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_isVisible_Injected(IntPtr _unity_self);

		// Token: 0x0600108C RID: 4236
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern ShadowCastingMode get_shadowCastingMode_Injected(IntPtr _unity_self);

		// Token: 0x0600108D RID: 4237
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_shadowCastingMode_Injected(IntPtr _unity_self, ShadowCastingMode value);

		// Token: 0x0600108E RID: 4238
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_receiveShadows_Injected(IntPtr _unity_self);

		// Token: 0x0600108F RID: 4239
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_receiveShadows_Injected(IntPtr _unity_self, bool value);

		// Token: 0x06001090 RID: 4240
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_forceRenderingOff_Injected(IntPtr _unity_self);

		// Token: 0x06001091 RID: 4241
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_forceRenderingOff_Injected(IntPtr _unity_self, bool value);

		// Token: 0x06001092 RID: 4242
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_allowGPUDrivenRendering_Injected(IntPtr _unity_self);

		// Token: 0x06001093 RID: 4243
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_allowGPUDrivenRendering_Injected(IntPtr _unity_self, bool value);

		// Token: 0x06001094 RID: 4244
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_smallMeshCulling_Injected(IntPtr _unity_self);

		// Token: 0x06001095 RID: 4245
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_smallMeshCulling_Injected(IntPtr _unity_self, bool value);

		// Token: 0x06001096 RID: 4246
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool GetIsStaticShadowCaster_Injected(IntPtr _unity_self);

		// Token: 0x06001097 RID: 4247
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetIsStaticShadowCaster_Injected(IntPtr _unity_self, bool value);

		// Token: 0x06001098 RID: 4248
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern MotionVectorGenerationMode get_motionVectorGenerationMode_Injected(IntPtr _unity_self);

		// Token: 0x06001099 RID: 4249
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_motionVectorGenerationMode_Injected(IntPtr _unity_self, MotionVectorGenerationMode value);

		// Token: 0x0600109A RID: 4250
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern LightProbeUsage get_lightProbeUsage_Injected(IntPtr _unity_self);

		// Token: 0x0600109B RID: 4251
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_lightProbeUsage_Injected(IntPtr _unity_self, LightProbeUsage value);

		// Token: 0x0600109C RID: 4252
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern ReflectionProbeUsage get_reflectionProbeUsage_Injected(IntPtr _unity_self);

		// Token: 0x0600109D RID: 4253
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_reflectionProbeUsage_Injected(IntPtr _unity_self, ReflectionProbeUsage value);

		// Token: 0x0600109E RID: 4254
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern uint get_renderingLayerMask_Injected(IntPtr _unity_self);

		// Token: 0x0600109F RID: 4255
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_renderingLayerMask_Injected(IntPtr _unity_self, uint value);

		// Token: 0x060010A0 RID: 4256
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int get_rendererPriority_Injected(IntPtr _unity_self);

		// Token: 0x060010A1 RID: 4257
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_rendererPriority_Injected(IntPtr _unity_self, int value);

		// Token: 0x060010A2 RID: 4258
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern RayTracingMode get_rayTracingMode_Injected(IntPtr _unity_self);

		// Token: 0x060010A3 RID: 4259
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_rayTracingMode_Injected(IntPtr _unity_self, RayTracingMode value);

		// Token: 0x060010A4 RID: 4260
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern RayTracingAccelerationStructureBuildFlags get_rayTracingAccelerationStructureBuildFlags_Injected(IntPtr _unity_self);

		// Token: 0x060010A5 RID: 4261
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_rayTracingAccelerationStructureBuildFlags_Injected(IntPtr _unity_self, RayTracingAccelerationStructureBuildFlags value);

		// Token: 0x060010A6 RID: 4262
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_rayTracingAccelerationStructureBuildFlagsOverride_Injected(IntPtr _unity_self);

		// Token: 0x060010A7 RID: 4263
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_rayTracingAccelerationStructureBuildFlagsOverride_Injected(IntPtr _unity_self, bool value);

		// Token: 0x060010A8 RID: 4264
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void get_sortingLayerName_Injected(IntPtr _unity_self, out ManagedSpanWrapper ret);

		// Token: 0x060010A9 RID: 4265
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_sortingLayerName_Injected(IntPtr _unity_self, ref ManagedSpanWrapper value);

		// Token: 0x060010AA RID: 4266
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int get_sortingLayerID_Injected(IntPtr _unity_self);

		// Token: 0x060010AB RID: 4267
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_sortingLayerID_Injected(IntPtr _unity_self, int value);

		// Token: 0x060010AC RID: 4268
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int get_sortingOrder_Injected(IntPtr _unity_self);

		// Token: 0x060010AD RID: 4269
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_sortingOrder_Injected(IntPtr _unity_self, int value);

		// Token: 0x060010AE RID: 4270
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern uint get_sortingKey_Injected(IntPtr _unity_self);

		// Token: 0x060010AF RID: 4271
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int get_sortingGroupID_Injected(IntPtr _unity_self);

		// Token: 0x060010B0 RID: 4272
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_sortingGroupID_Injected(IntPtr _unity_self, int value);

		// Token: 0x060010B1 RID: 4273
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int get_sortingGroupOrder_Injected(IntPtr _unity_self);

		// Token: 0x060010B2 RID: 4274
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_sortingGroupOrder_Injected(IntPtr _unity_self, int value);

		// Token: 0x060010B3 RID: 4275
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern uint get_sortingGroupKey_Injected(IntPtr _unity_self);

		// Token: 0x060010B4 RID: 4276
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_isLOD0_Injected(IntPtr _unity_self);

		// Token: 0x060010B5 RID: 4277
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_allowOcclusionWhenDynamic_Injected(IntPtr _unity_self);

		// Token: 0x060010B6 RID: 4278
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_allowOcclusionWhenDynamic_Injected(IntPtr _unity_self, bool value);

		// Token: 0x060010B7 RID: 4279
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr get_staticBatchRootTransform_Injected(IntPtr _unity_self);

		// Token: 0x060010B8 RID: 4280
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_staticBatchRootTransform_Injected(IntPtr _unity_self, IntPtr value);

		// Token: 0x060010B9 RID: 4281
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int get_staticBatchIndex_Injected(IntPtr _unity_self);

		// Token: 0x060010BA RID: 4282
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetStaticBatchInfo_Injected(IntPtr _unity_self, int firstSubMesh, int subMeshCount);

		// Token: 0x060010BB RID: 4283
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_isPartOfStaticBatch_Injected(IntPtr _unity_self);

		// Token: 0x060010BC RID: 4284
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void get_worldToLocalMatrix_Injected(IntPtr _unity_self, out Matrix4x4 ret);

		// Token: 0x060010BD RID: 4285
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void get_localToWorldMatrix_Injected(IntPtr _unity_self, out Matrix4x4 ret);

		// Token: 0x060010BE RID: 4286
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr get_lightProbeProxyVolumeOverride_Injected(IntPtr _unity_self);

		// Token: 0x060010BF RID: 4287
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_lightProbeProxyVolumeOverride_Injected(IntPtr _unity_self, IntPtr value);

		// Token: 0x060010C0 RID: 4288
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr get_probeAnchor_Injected(IntPtr _unity_self);

		// Token: 0x060010C1 RID: 4289
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void set_probeAnchor_Injected(IntPtr _unity_self, IntPtr value);

		// Token: 0x060010C2 RID: 4290
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int GetLightmapIndex_Injected(IntPtr _unity_self, LightmapType lt);

		// Token: 0x060010C3 RID: 4291
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetLightmapIndex_Injected(IntPtr _unity_self, int index, LightmapType lt);

		// Token: 0x060010C4 RID: 4292
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetLightmapST_Injected(IntPtr _unity_self, LightmapType lt, out Vector4 ret);

		// Token: 0x060010C5 RID: 4293
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetLightmapST_Injected(IntPtr _unity_self, [In] ref Vector4 st, LightmapType lt);

		// Token: 0x060010C6 RID: 4294
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int GetMaterialCount_Injected(IntPtr _unity_self);

		// Token: 0x060010C7 RID: 4295
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Material[] GetSharedMaterialArray_Injected(IntPtr _unity_self);

		// Token: 0x060010C8 RID: 4296
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr get_LODGroup_Injected(IntPtr _unity_self);
	}
}
