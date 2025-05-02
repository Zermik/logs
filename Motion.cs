using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine
{
	// Token: 0x02000040 RID: 64
	[NativeHeader("Modules/Animation/Motion.h")]
	public class Motion : Object
	{
		// Token: 0x06000387 RID: 903 RVA: 0x00006C0A File Offset: 0x00004E0A
		protected Motion()
		{
		}

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x06000388 RID: 904 RVA: 0x00007B28 File Offset: 0x00005D28
		public float averageDuration
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Motion>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Motion.get_averageDuration_Injected(intPtr);
			}
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000389 RID: 905 RVA: 0x00007B4C File Offset: 0x00005D4C
		public float averageAngularSpeed
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Motion>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Motion.get_averageAngularSpeed_Injected(intPtr);
			}
		}

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x0600038A RID: 906 RVA: 0x00007B70 File Offset: 0x00005D70
		public Vector3 averageSpeed
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Motion>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				Vector3 result;
				Motion.get_averageSpeed_Injected(intPtr, out result);
				return result;
			}
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x0600038B RID: 907 RVA: 0x00007B98 File Offset: 0x00005D98
		public float apparentSpeed
		{
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Motion>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Motion.get_apparentSpeed_Injected(intPtr);
			}
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x0600038C RID: 908 RVA: 0x00007BBC File Offset: 0x00005DBC
		public bool isLooping
		{
			[NativeMethod("IsLooping")]
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Motion>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Motion.get_isLooping_Injected(intPtr);
			}
		}

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x0600038D RID: 909 RVA: 0x00007BE0 File Offset: 0x00005DE0
		public bool legacy
		{
			[NativeMethod("IsLegacy")]
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Motion>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Motion.get_legacy_Injected(intPtr);
			}
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x0600038E RID: 910 RVA: 0x00007C04 File Offset: 0x00005E04
		public bool isHumanMotion
		{
			[NativeMethod("IsHumanMotion")]
			get
			{
				IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull<Motion>(this);
				if (intPtr == 0)
				{
					ThrowHelper.ThrowNullReferenceException(this);
				}
				return Motion.get_isHumanMotion_Injected(intPtr);
			}
		}

		// Token: 0x0600038F RID: 911 RVA: 0x00007C28 File Offset: 0x00005E28
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("ValidateIfRetargetable is not supported anymore, please use isHumanMotion instead.", true)]
		public bool ValidateIfRetargetable(bool val)
		{
			return false;
		}

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x06000390 RID: 912 RVA: 0x00007C3B File Offset: 0x00005E3B
		[Obsolete("isAnimatorMotion is not supported anymore, please use !legacy instead.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool isAnimatorMotion { get; }

		// Token: 0x06000391 RID: 913
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern float get_averageDuration_Injected(IntPtr _unity_self);

		// Token: 0x06000392 RID: 914
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern float get_averageAngularSpeed_Injected(IntPtr _unity_self);

		// Token: 0x06000393 RID: 915
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void get_averageSpeed_Injected(IntPtr _unity_self, out Vector3 ret);

		// Token: 0x06000394 RID: 916
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern float get_apparentSpeed_Injected(IntPtr _unity_self);

		// Token: 0x06000395 RID: 917
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_isLooping_Injected(IntPtr _unity_self);

		// Token: 0x06000396 RID: 918
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_legacy_Injected(IntPtr _unity_self);

		// Token: 0x06000397 RID: 919
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool get_isHumanMotion_Injected(IntPtr _unity_self);
	}
}
