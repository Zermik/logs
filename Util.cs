using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using BrokeProtocol.Client.Builder;
using BrokeProtocol.Collections;
using BrokeProtocol.Entities;
using BrokeProtocol.Managers;
using BrokeProtocol.Required;
using BrokeProtocol.Utility.Jobs;
using BrokeProtocol.Utility.Networking;
using ENet;
using Pathfinding;
using Pathfinding.Graphs.Navmesh;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace BrokeProtocol.Utility
{
	// Token: 0x0200006D RID: 109
	public static class Util
	{
		// Token: 0x060000BA RID: 186 RVA: 0x00005018 File Offset: 0x00003218
		public static T Increment<T>(this T enumValue) where T : struct, Enum
		{
			Array values = Enum.GetValues(enumValue.GetType());
			int num = Array.IndexOf(values, enumValue) + 1;
			if (num >= values.Length)
			{
				num = 0;
			}
			return (T)((object)values.GetValue(num));
		}

		// Token: 0x060000BB RID: 187 RVA: 0x0000505E File Offset: 0x0000325E
		public static bool IsZero(this float d)
		{
			return d == 0f;
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00005068 File Offset: 0x00003268
		public static bool IsZero(this Vector2 v)
		{
			return v.sqrMagnitude.IsZero();
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00005076 File Offset: 0x00003276
		public static bool IsZero(this Vector3 v)
		{
			return v.sqrMagnitude.IsZero();
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00005084 File Offset: 0x00003284
		public static bool IsNaN(this Vector2 v)
		{
			return float.IsNaN(v.x) || float.IsNaN(v.y);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x000050A0 File Offset: 0x000032A0
		public static bool IsNaN(this Vector3 v)
		{
			return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x000050CC File Offset: 0x000032CC
		public static int SolveQuadric(float[] c, ref float[] s)
		{
			if (s == null)
			{
				s = new float[2];
			}
			float num = c[1] / (2f * c[2]);
			float num2 = c[0] / c[2];
			float num3 = num * num - num2;
			if (num3.IsZero())
			{
				s[0] = -num;
				return 1;
			}
			if (num3 < 0f)
			{
				return 0;
			}
			float num4 = MathF.Sqrt(num3);
			s[0] = num4 - num;
			s[1] = -num4 - num;
			return 2;
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00005134 File Offset: 0x00003334
		public static int SolveCubic(float[] c, ref float[] s)
		{
			if (s == null)
			{
				s = new float[3];
			}
			float num = c[2] / c[3];
			float num2 = c[1] / c[3];
			float num3 = c[0] / c[3];
			float num4 = num * num;
			float num5 = 0.33333334f * (-0.33333334f * num4 + num2);
			float num6 = 0.5f * (0.074074075f * num * num4 - 0.33333334f * num * num2 + num3);
			float num7 = num5 * num5 * num5;
			float num8 = num6 * num6 + num7;
			int num9;
			if (num8.IsZero())
			{
				if (num6.IsZero())
				{
					s[0] = 0f;
					num9 = 1;
				}
				else
				{
					float num10 = MathF.Cbrt(-num6);
					s[0] = 2f * num10;
					s[1] = -num10;
					num9 = 2;
				}
			}
			else if (num8 < 0f)
			{
				float num11 = 0.33333334f * MathF.Acos(-num6 / MathF.Sqrt(-num7));
				float num12 = 2f * MathF.Sqrt(-num5);
				s[0] = num12 * MathF.Cos(num11);
				s[1] = -num12 * MathF.Cos(num11 + 1.0471976f);
				s[2] = -num12 * MathF.Cos(num11 - 1.0471976f);
				num9 = 3;
			}
			else
			{
				float num13 = MathF.Sqrt(num8);
				float num14 = MathF.Cbrt(num13 - num6);
				float num15 = -MathF.Cbrt(num13 + num6);
				s[0] = num14 + num15;
				num9 = 1;
			}
			float num16 = 0.33333334f * num;
			for (int i = 0; i < num9; i++)
			{
				s[i] -= num16;
			}
			return num9;
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x000052B8 File Offset: 0x000034B8
		public static int SolveQuartic(float[] c, ref float[] s)
		{
			if (s == null)
			{
				s = new float[4];
			}
			float num = c[3] / c[4];
			float num2 = c[2] / c[4];
			float num3 = c[1] / c[4];
			float num4 = c[0] / c[4];
			float num5 = num * num;
			float num6 = -0.375f * num5 + num2;
			float num7 = 0.125f * num5 * num - 0.5f * num * num2 + num3;
			float num8 = -0.01171875f * num5 * num5 + 0.0625f * num5 * num2 - 0.25f * num * num3 + num4;
			float[] array = new float[4];
			int num9;
			if (num8.IsZero())
			{
				array[0] = num7;
				array[1] = num6;
				array[2] = 0f;
				array[3] = 1f;
				num9 = Util.SolveCubic(array, ref s);
				s[num9++] = 0f;
			}
			else
			{
				array[0] = 0.5f * num8 * num6 - 0.125f * num7 * num7;
				array[1] = -num8;
				array[2] = -0.5f * num6;
				array[3] = 1f;
				Util.SolveCubic(array, ref s);
				float num10 = s[0];
				float num11 = num10 * num10 - num8;
				if (num11 > 0f)
				{
					num11 = MathF.Sqrt(num11);
				}
				else if (num11 < 0f)
				{
					return 0;
				}
				float num12 = 2f * num10 - num6;
				if (num12 > 0f)
				{
					num12 = MathF.Sqrt(num12);
				}
				else if (num12 < 0f)
				{
					return 0;
				}
				array[0] = num10 - num11;
				array[1] = ((num7 < 0f) ? (-num12) : num12);
				array[2] = 1f;
				num9 = Util.SolveQuadric(array, ref s);
				array[0] = num10 + num11;
				array[1] = ((num7 < 0f) ? num12 : (-num12));
				array[2] = 1f;
				float[] array2 = null;
				int num13 = Util.SolveQuadric(array, ref array2);
				for (int i = 0; i < num13; i++)
				{
					s[i + num9] = array2[i];
				}
				num9 += num13;
			}
			float num14 = 0.25f * num;
			for (int j = 0; j < num9; j++)
			{
				s[j] -= num14;
			}
			return num9;
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x000054DA File Offset: 0x000036DA
		public static float BallisticRange(float speed, float gravity)
		{
			if (gravity < 0f)
			{
				return MathF.Min(362.03867f, speed * speed / -gravity);
			}
			return 362.03867f;
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x000054FC File Offset: 0x000036FC
		public static bool AimVector(Vector3 deltaP, Vector3 deltaV, float speed, float gravity, bool lob, out Vector3 aim)
		{
			float sqrMagnitude = deltaP.sqrMagnitude;
			if (sqrMagnitude < 1f)
			{
				aim = Vector3.forward;
				return true;
			}
			float num = MathF.Sqrt(sqrMagnitude);
			if (!speed.IsZero() && num / speed < 0.1f)
			{
				aim = deltaP / num;
				return true;
			}
			float num2 = speed * speed;
			float sqrMagnitude2 = deltaV.sqrMagnitude;
			if (gravity.IsZero())
			{
				float[] array = new float[]
				{
					0f,
					0f,
					sqrMagnitude2 - num2
				};
				array[1] = 2f * (deltaP.x * deltaV.x + deltaP.y * deltaV.y + deltaP.z * deltaV.z);
				array[0] = sqrMagnitude;
				float[] array2 = new float[2];
				int num3 = Util.SolveQuadric(array, ref array2);
				float num4;
				if (Util.GetSolution(ref array2, num3, lob, out num4))
				{
					aim = (deltaV * num4 + deltaP) / (num4 * speed);
					return true;
				}
				aim = deltaP / num;
				return false;
			}
			else if (sqrMagnitude2 < 1f)
			{
				Vector3 a = new Vector3(deltaP.x, 0f, deltaP.z);
				float magnitude = a.magnitude;
				float num5 = -gravity * magnitude;
				float num6 = num2 * num2 + gravity * (num5 * magnitude + 2f * deltaP.y * num2);
				if (num6 <= 0f)
				{
					aim = deltaP / num;
					return false;
				}
				float x = MathF.Atan2(num2 - MathF.Sqrt(num6), num5);
				aim = a / magnitude * MathF.Cos(x) + MathF.Sin(x) * Vector3.up;
				return true;
			}
			else
			{
				float num7 = 0.5f * gravity;
				float[] array3 = new float[]
				{
					0f,
					0f,
					0f,
					0f,
					num7 * num7
				};
				array3[3] = -2f * deltaV.y * num7;
				array3[2] = sqrMagnitude2 - 2f * deltaP.y * num7 - num2;
				array3[1] = 2f * (deltaP.x * deltaV.x + deltaP.y * deltaV.y + deltaP.z * deltaV.z);
				array3[0] = sqrMagnitude;
				float[] array4 = null;
				int num8 = Util.SolveQuartic(array3, ref array4);
				float num9;
				if (Util.GetSolution(ref array4, num8, lob, out num9))
				{
					aim = (deltaP + num9 * new Vector3(deltaV.x, deltaV.y - num7 * num9, deltaV.z)) / (num9 * speed);
					return true;
				}
				aim = deltaP / num;
				return false;
			}
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00005784 File Offset: 0x00003984
		private static bool GetSolution(ref float[] solutions, int num, bool lob, out float solution)
		{
			solution = solutions[0];
			for (int i = 1; i < num; i++)
			{
				float num2 = solutions[i];
				if (solution <= 0f || (num2 > 0f && (lob ? (num2 > solution) : (num2 < solution))))
				{
					solution = num2;
				}
			}
			return solution > 0f;
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x000057D6 File Offset: 0x000039D6
		public static float SafePow(float x, float y)
		{
			if (x < 0f)
			{
				return -Mathf.Pow(-x, y);
			}
			return Mathf.Pow(x, y);
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x000057F4 File Offset: 0x000039F4
		public static string ToStringKB(this int length)
		{
			return (length / 1024).ToString() + " KB";
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x0000581C File Offset: 0x00003A1C
		public static Color FractionToColor(this float percent)
		{
			if (percent < 0.5f)
			{
				float num = Mathf.Lerp(1f, 0f, percent * 2f);
				return new Color(1f, num, num);
			}
			return new Color(Mathf.Lerp(1f, 0f, (percent - 0.5f) * 2f), 0f, 0f);
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00005880 File Offset: 0x00003A80
		static Util()
		{
			Random.InitState(123);
			for (int i = 0; i < Util.randomVector.Length; i++)
			{
				Util.randomVector[i] = Random.insideUnitCircle;
			}
			Random.InitState((int)Util.CurrentTime.Ticks);
			for (int j = 0; j <= 128; j++)
			{
				float num = (float)j / 128f;
				Util.sinArray[j] = Mathf.Sin(6.2831855f * num);
				Util.cosArray[j] = Mathf.Cos(6.2831855f * num);
				num = num * 2f - 1f;
				Util.aSinArray[j] = Mathf.Asin(num);
				Util.aCosArray[j] = Mathf.Acos(num);
			}
			Util.itemTypes = Util.GetAllSubclasses(typeof(ShItem));
		}

		// Token: 0x060000CA RID: 202 RVA: 0x00005D5A File Offset: 0x00003F5A
		public static float FastSin(this float angle)
		{
			return Util.sinArray[Mathf.RoundToInt(128f * angle.Mod(6.2831855f) / 6.2831855f).Clamp(0, 128)];
		}

		// Token: 0x060000CB RID: 203 RVA: 0x00005D89 File Offset: 0x00003F89
		public static float FastCos(this float angle)
		{
			return Util.cosArray[Mathf.RoundToInt(128f * angle.Mod(6.2831855f) / 6.2831855f).Clamp(0, 128)];
		}

		// Token: 0x060000CC RID: 204 RVA: 0x00005DB8 File Offset: 0x00003FB8
		public static float FastASin(this float x)
		{
			return Util.aSinArray[Mathf.RoundToInt(128f * (x * 0.5f + 0.5f)).Clamp(0, 128)];
		}

		// Token: 0x060000CD RID: 205 RVA: 0x00005DE3 File Offset: 0x00003FE3
		public static float FastACos(this float x)
		{
			return Util.aCosArray[Mathf.RoundToInt(128f * (x * 0.5f + 0.5f)).Clamp(0, 128)];
		}

		// Token: 0x060000CE RID: 206 RVA: 0x00005E0E File Offset: 0x0000400E
		public static float Pow2(this float x)
		{
			return x * x;
		}

		// Token: 0x060000CF RID: 207 RVA: 0x00005E13 File Offset: 0x00004013
		public static int Clamp(this int n, int min, int max)
		{
			if (n < min)
			{
				return min;
			}
			if (n > max)
			{
				return max;
			}
			return n;
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x00005E22 File Offset: 0x00004022
		public static Vector3 OffsetToDirection(this Transform transform, Vector2 offset)
		{
			return transform.TransformDirection(Quaternion.Euler(offset) * Vector3.forward);
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x00005E40 File Offset: 0x00004040
		public static Quaternion GetFireOffset(this WeaponSet weaponSet, Transform transform)
		{
			Vector2 offset = weaponSet.accuracy * Util.randomVector[(weaponSet.curAmmo + transform.GetHashCode()).Mod(Util.randomVector.Length)];
			return Quaternion.LookRotation(transform.OffsetToDirection(offset), transform.up);
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x00005E90 File Offset: 0x00004090
		public static string RandomURL(this string url)
		{
			return url + "?p=" + Random.Range(0, 999).ToString();
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x00005EBB File Offset: 0x000040BB
		public static int Mod(this int a, int b)
		{
			return (a % b + b) % b;
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x00005EC4 File Offset: 0x000040C4
		public static float Mod(this float a, float b)
		{
			return a - b * Mathf.Floor(a / b);
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x00005ED4 File Offset: 0x000040D4
		public static void SetLayer(this Transform t, int layer)
		{
			t.gameObject.layer = layer;
			foreach (object obj in t)
			{
				((Transform)obj).SetLayer(layer);
			}
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x00005F34 File Offset: 0x00004134
		public static void SetTRS(this Transform t, TransformStruct ts)
		{
			t.SetPositionAndRotation(ts.position, ts.rotation);
			t.localScale = ts.scale;
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x00005F54 File Offset: 0x00004154
		public static float MinDistanceToBounds(this Camera cam, Bounds bounds)
		{
			return Mathf.Max(new float[]
			{
				bounds.size.x,
				bounds.size.y,
				bounds.size.z
			}) / Mathf.Tan(cam.fieldOfView * 0.5f * 0.017453292f);
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x00005FB1 File Offset: 0x000041B1
		public static Type PropertyTypeFromEntityName(this string name)
		{
			return Type.GetType("BrokeProtocol.Properties." + name + "Properties");
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x00005FC8 File Offset: 0x000041C8
		public static Type PropertyScriptFromEntity<T>(this GameObject g) where T : MonoBehaviour
		{
			T t;
			if (!g.TryGetComponent<T>(out t))
			{
				return null;
			}
			return t.GetType().Name.PropertyTypeFromEntityName();
		}

		// Token: 0x060000DA RID: 218 RVA: 0x00005FF6 File Offset: 0x000041F6
		public static Type SerializedScriptFromProperty(this Type t)
		{
			return Type.GetType("BrokeProtocol.Entities." + t.Name.Replace("Properties", string.Empty));
		}

		// Token: 0x060000DB RID: 219 RVA: 0x0000601C File Offset: 0x0000421C
		public static Type EntityScriptFromReference(this Type t)
		{
			return Type.GetType("BrokeProtocol.Entities." + t.Name.Replace("Reference", "Entity"));
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00006042 File Offset: 0x00004242
		public static int GetPrefabIndex(this GameObject g)
		{
			return Animator.StringToHash(g.name);
		}

		// Token: 0x060000DD RID: 221 RVA: 0x0000604F File Offset: 0x0000424F
		public static int GetPrefabIndex(this Transform t)
		{
			return Animator.StringToHash(t.name);
		}

		// Token: 0x060000DE RID: 222 RVA: 0x0000605C File Offset: 0x0000425C
		public static int GetPrefabIndex(this string s)
		{
			return Animator.StringToHash(s);
		}

		// Token: 0x060000DF RID: 223 RVA: 0x00006064 File Offset: 0x00004264
		public static void InitializeEditor(this GameObject g)
		{
			if (!g.GetComponentInChildren<Renderer>())
			{
				g.AddComponent<BlGizmoIcon>();
			}
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x0000607A File Offset: 0x0000427A
		public static void Reset(this ParticleSystem p)
		{
			p.Stop();
			p.Clear();
			p.Play();
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x0000608E File Offset: 0x0000428E
		public static BoundsHighlight BoundsHighlightAdd(this GameObject g, Bounds bounds)
		{
			BoundsHighlight boundsHighlight = g.AddComponent<BoundsHighlight>();
			boundsHighlight.bounds = bounds;
			return boundsHighlight;
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x000060A0 File Offset: 0x000042A0
		public static Bounds Intersection(Bounds boundsA, Bounds boundsB)
		{
			if (boundsA.Intersects(boundsB))
			{
				Vector3 min = Vector3.Max(boundsA.min, boundsB.min);
				Vector3 max = Vector3.Min(boundsA.max, boundsB.max);
				Bounds result = default(Bounds);
				result.SetMinMax(min, max);
				return result;
			}
			return default(Bounds);
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x000060FB File Offset: 0x000042FB
		public static void BoundsHighlightRemove(this GameObject g)
		{
			Object.Destroy(g.GetComponent<BoundsHighlight>());
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00006108 File Offset: 0x00004308
		public static void DeltaRotate(this Transform t, float deltaX, float deltaY, float limit = 89f)
		{
			Vector3 localEulerAngles = t.localEulerAngles;
			localEulerAngles.x = Mathf.Clamp(Mathf.DeltaAngle(0f, localEulerAngles.x - deltaY), -limit, limit);
			localEulerAngles.y += deltaX;
			t.localRotation = Quaternion.Euler(localEulerAngles);
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x00006158 File Offset: 0x00004358
		public static void LimitEuler(this Transform t, float limit)
		{
			Vector3 localEulerAngles = t.localEulerAngles;
			localEulerAngles.x = Mathf.Clamp(Mathf.DeltaAngle(0f, localEulerAngles.x), -limit, limit);
			localEulerAngles.y = Mathf.Clamp(Mathf.DeltaAngle(0f, localEulerAngles.y), -limit, limit);
			localEulerAngles.z = Mathf.Clamp(Mathf.DeltaAngle(0f, localEulerAngles.z), -limit, limit);
			t.localRotation = Quaternion.Euler(localEulerAngles);
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x000061D5 File Offset: 0x000043D5
		public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
		{
			return a + t * (b - a);
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x000061EC File Offset: 0x000043EC
		public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
		{
			Vector3 vector = b - a;
			Vector3 lhs = Vector3.Project(value - a, vector);
			return lhs.magnitude / vector.magnitude * Mathf.Sign(Vector3.Dot(lhs, vector));
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x0000622C File Offset: 0x0000442C
		public static LayerMask GetCollisionMask(this GameObject go)
		{
			int layer = go.layer;
			int num = 0;
			int num2 = 0;
			while (num2 < 6 || LayerMask.LayerToName(num2) != string.Empty)
			{
				if (!Physics.GetIgnoreLayerCollision(layer, num2))
				{
					num |= 1 << num2;
				}
				num2++;
			}
			return num;
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x00006278 File Offset: 0x00004478
		public static Bounds GetWorldBounds(this Transform t)
		{
			Bounds result = default(Bounds);
			foreach (MeshRenderer meshRenderer in t.GetComponentsInChildren<MeshRenderer>(false))
			{
				if (result.extents == Vector3.zero)
				{
					result = meshRenderer.bounds;
				}
				else if (meshRenderer.bounds.extents != Vector3.zero)
				{
					result.Encapsulate(meshRenderer.bounds);
				}
			}
			if (result.extents == Vector3.zero)
			{
				foreach (Collider collider in t.GetComponentsInChildren<Collider>(true))
				{
					if (result.extents == Vector3.zero)
					{
						result = collider.bounds;
					}
					else
					{
						result.Encapsulate(collider.bounds);
					}
				}
			}
			if (result.extents == Vector3.zero)
			{
				result = new Bounds(t.position, Vector3.one);
			}
			return result;
		}

		// Token: 0x060000EA RID: 234 RVA: 0x0000636D File Offset: 0x0000456D
		public static Vector3 Inverse(this Vector3 v)
		{
			return new Vector3(1f / v.x, 1f / v.y, 1f / v.z);
		}

		// Token: 0x060000EB RID: 235 RVA: 0x00006398 File Offset: 0x00004598
		public static Bounds TransformBounds(Transform t)
		{
			Bounds bounds = default(Bounds);
			foreach (object obj in t)
			{
				Transform transform = (Transform)obj;
				if (!transform.GetComponent<ShEntity>())
				{
					foreach (Collider collider in transform.GetComponents<Collider>())
					{
						if (bounds == default(Bounds))
						{
							bounds = collider.bounds;
						}
						else
						{
							bounds.Encapsulate(collider.bounds);
						}
					}
				}
			}
			return bounds;
		}

		// Token: 0x060000EC RID: 236 RVA: 0x00006448 File Offset: 0x00004648
		private static Bounds TransformedBounds(Bounds localBounds, Transform mainT, Transform meshT)
		{
			Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, meshT.rotation, meshT.lossyScale);
			Vector3 vector = matrix4x.MultiplyVector(localBounds.size);
			Vector3 b = matrix4x.MultiplyPoint3x4(localBounds.center);
			return new Bounds(mainT.InverseTransformPoint(meshT.position + b), mainT.InverseTransformVector(vector));
		}

		// Token: 0x060000ED RID: 237 RVA: 0x000064A8 File Offset: 0x000046A8
		public static Bounds GetLocalBounds(this Serialized s, bool forceStatic)
		{
			Bounds result = default(Bounds);
			SkinnedMeshRenderer skinnedMeshRenderer;
			if (s.TryGetComponent<SkinnedMeshRenderer>(out skinnedMeshRenderer))
			{
				Transform transform = skinnedMeshRenderer.transform;
				Mesh mesh;
				if (forceStatic && s.go.GetMesh(out mesh))
				{
					result = Util.TransformedBounds(mesh.bounds, s.mainT, transform);
				}
				else
				{
					result = Util.TransformedBounds(skinnedMeshRenderer.localBounds, s.mainT, transform);
				}
			}
			else
			{
				foreach (MeshFilter meshFilter in s.GetComponentsInChildren<MeshFilter>())
				{
					Mesh sharedMesh = meshFilter.sharedMesh;
					if (sharedMesh)
					{
						result.Encapsulate(Util.TransformedBounds(sharedMesh.bounds, s.mainT, meshFilter.transform));
					}
				}
			}
			if (result.extents == Vector3.zero)
			{
				result = new Bounds(Vector3.zero, Vector3.one);
			}
			return result;
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00006583 File Offset: 0x00004783
		public static void ParseOptionAction(out int targetID, out string id, out string optionID, out string actionID)
		{
			targetID = Buffers.reader.ReadInt32();
			id = Buffers.reader.ReadString();
			optionID = Buffers.reader.ReadString();
			actionID = Buffers.reader.ReadString();
		}

		// Token: 0x060000EF RID: 239 RVA: 0x000065B5 File Offset: 0x000047B5
		public static void ParseTextPanelButton(out string id, out string optionID)
		{
			id = Buffers.reader.ReadString();
			optionID = Buffers.reader.ReadString();
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x000065CF File Offset: 0x000047CF
		public static void ParseSubmitInput(out int targetID, out string id, out string input)
		{
			targetID = Buffers.reader.ReadInt32();
			id = Buffers.reader.ReadString();
			input = Buffers.reader.ReadString();
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x000065F8 File Offset: 0x000047F8
		public static bool GetMesh(this GameObject go, out Mesh mesh)
		{
			MeshFilter meshFilter;
			if (go.TryGetComponent<MeshFilter>(out meshFilter))
			{
				mesh = meshFilter.sharedMesh;
				return true;
			}
			MeshFilter meshFilter2;
			if (go.ConvertSkinnedMeshRenderer(out meshFilter2, true))
			{
				mesh = meshFilter2.sharedMesh;
				return true;
			}
			mesh = null;
			return false;
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00006634 File Offset: 0x00004834
		public static bool ConvertSkinnedMeshRenderer(this GameObject go, out MeshFilter meshFilter, bool destroyOriginal = true)
		{
			SkinnedMeshRenderer skinnedMeshRenderer;
			if (go.TryGetComponent<SkinnedMeshRenderer>(out skinnedMeshRenderer))
			{
				Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
				Material[] sharedMaterials = skinnedMeshRenderer.sharedMaterials;
				if (destroyOriginal)
				{
					skinnedMeshRenderer.enabled = false;
					Cloth obj;
					if (go.TryGetComponent<Cloth>(out obj))
					{
						Object.Destroy(obj);
					}
					Object.Destroy(skinnedMeshRenderer);
				}
				GameObject gameObject = new GameObject("StaticMesh");
				gameObject.transform.SetParent(go.transform, false);
				meshFilter = gameObject.AddComponent<MeshFilter>();
				meshFilter.sharedMesh = sharedMesh;
				MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
				meshRenderer.sharedMaterials = sharedMaterials;
				meshRenderer.FixRenderer();
				return true;
			}
			meshFilter = null;
			return false;
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x000066C0 File Offset: 0x000048C0
		public static void DestroyExtras(this Transform t)
		{
			Component[] components = t.GetComponents(typeof(Component));
			Util.ActionOnExtras(components, new Action<Component>(Util.DestroyRequirements));
			Util.ActionOnExtras(components, delegate(Component c)
			{
				if (c)
				{
					Object.Destroy(c);
				}
			});
			foreach (object obj in t)
			{
				((Transform)obj).DestroyExtras();
			}
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00006758 File Offset: 0x00004958
		private static void ActionOnExtras(Component[] components, Action<Component> action)
		{
			foreach (Component component in components)
			{
				if (!(component is UIBehaviour))
				{
					if (!(component is MonoBehaviour) && !(component is Animator) && !(component is Rigidbody) && !(component is Collider) && !(component is Transform) && !(component is Joint) && !(component is MeshFilter))
					{
						action(component);
					}
				}
				else
				{
					action(component);
				}
			}
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x000067CC File Offset: 0x000049CC
		private static void DestroyRequirements(Component c)
		{
			foreach (RequireComponent requireComponent in c.GetType().GetCustomAttributes(typeof(RequireComponent), false) as RequireComponent[])
			{
				if ((requireComponent.m_Type0 != null && !requireComponent.m_Type0.IsSubclassOf(typeof(Transform))) || (requireComponent.m_Type1 != null && !requireComponent.m_Type1.IsSubclassOf(typeof(Transform))) || (requireComponent.m_Type2 != null && !requireComponent.m_Type2.IsSubclassOf(typeof(Transform))))
				{
					Object.Destroy(c);
					return;
				}
			}
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x00006880 File Offset: 0x00004A80
		public static void DestroyDecorations(this MonoBehaviour g)
		{
			foreach (Graphic graphic in g.GetComponentsInChildren<Graphic>())
			{
				if (graphic && graphic.canvas)
				{
					Object.Destroy(graphic.canvas.gameObject);
				}
			}
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x000068CC File Offset: 0x00004ACC
		public static string GetChecksum(this byte[] data)
		{
			StringBuilder stringBuilder = new StringBuilder();
			using (MD5 md = MD5.Create())
			{
				foreach (byte b in md.ComputeHash(data))
				{
					stringBuilder.Append(b.ToString("x2"));
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x00006938 File Offset: 0x00004B38
		public static bool TryParseInt(this string value, out int result)
		{
			return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x0000694B File Offset: 0x00004B4B
		public static string ToStringSafe(this int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x060000FA RID: 250 RVA: 0x00006959 File Offset: 0x00004B59
		public static bool TryParseByte(this string value, out byte result)
		{
			return byte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
		}

		// Token: 0x060000FB RID: 251 RVA: 0x0000696C File Offset: 0x00004B6C
		public static string ToStringSafe(this byte value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x060000FC RID: 252 RVA: 0x0000697A File Offset: 0x00004B7A
		public static bool TryParseFloat(this string value, out float result)
		{
			return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
		}

		// Token: 0x060000FD RID: 253 RVA: 0x0000698D File Offset: 0x00004B8D
		public static string ToStringSafe(this float value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x060000FE RID: 254 RVA: 0x0000699C File Offset: 0x00004B9C
		public static string ParseColorCodes(this string str)
		{
			if (string.IsNullOrWhiteSpace(str) || str.Length <= 1 || !str.Contains('&'.ToString()))
			{
				return str;
			}
			string[] array = str.Split(Util.colorSeparators, StringSplitOptions.RemoveEmptyEntries);
			StringBuilder stringBuilder = new StringBuilder();
			int i;
			if (str[0] == '&')
			{
				i = 0;
			}
			else
			{
				i = 1;
				stringBuilder.Append(array[0]);
			}
			while (i < array.Length)
			{
				string text = array[i];
				if (!string.IsNullOrWhiteSpace(text))
				{
					char c = text[0];
					string color;
					if (c == 'f')
					{
						StringBuilder stringBuilder2 = stringBuilder;
						string text2 = text;
						stringBuilder2.Append(text2.Substring(1, text2.Length - 1));
					}
					else if (Util.colorValues.TryGetValue(c, out color))
					{
						StringBuilder sb = stringBuilder;
						string text2 = text;
						sb.AppendColorText(text2.Substring(1, text2.Length - 1), color);
					}
					else
					{
						stringBuilder.Append(text);
					}
				}
				i++;
			}
			return stringBuilder.ToString();
		}

		// Token: 0x060000FF RID: 255 RVA: 0x00006A86 File Offset: 0x00004C86
		public static StringBuilder AppendColorText(this StringBuilder sb, string text, string color)
		{
			sb.Append("<color=#").Append(color).Append(">").Append(text).Append("</color>");
			return sb;
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00006AB8 File Offset: 0x00004CB8
		public static StringBuilder AppendColorText(this StringBuilder sb, string text, Color color)
		{
			string color2 = ColorUtility.ToHtmlStringRGB(color);
			sb.AppendColorText(text, color2);
			return sb;
		}

		// Token: 0x06000101 RID: 257 RVA: 0x00006AD8 File Offset: 0x00004CD8
		public static string CleanString(this string input, HashSet<char> badChars, int maxLength = 2147483647)
		{
			char[] array = new char[input.Length];
			int num = 0;
			foreach (char c in input)
			{
				if (!badChars.Contains(c))
				{
					array[num++] = c;
					if (num == maxLength)
					{
						break;
					}
				}
			}
			return new string(array, 0, num).Trim();
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00006B33 File Offset: 0x00004D33
		public static string CleanMessage(this string s)
		{
			return s.CleanString(Util.badMessageChars, 64);
		}

		// Token: 0x06000103 RID: 259 RVA: 0x00006B42 File Offset: 0x00004D42
		public static string CleanCredential(this string s)
		{
			return s.CleanString(Util.badCredentialChars, int.MaxValue);
		}

		// Token: 0x06000104 RID: 260 RVA: 0x00006B54 File Offset: 0x00004D54
		public static string CleanProfile(this string s)
		{
			if (s.Length > 128)
			{
				s = s.Substring(0, 128);
			}
			return s.Trim();
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00006B77 File Offset: 0x00004D77
		public static string PrefixChatString(this string s, ChatMode mode)
		{
			return string.Format("<<color=#7f7f7f>{0}</color>>{1}", mode, s);
		}

		// Token: 0x06000106 RID: 262 RVA: 0x00006B8C File Offset: 0x00004D8C
		public static T GetRandom<T>(this T[] arr)
		{
			int num = arr.Length;
			if (num == 0)
			{
				return default(T);
			}
			return arr[Random.Range(0, num)];
		}

		// Token: 0x06000107 RID: 263 RVA: 0x00006BB8 File Offset: 0x00004DB8
		public static T GetRandom<T>(this IEnumerable<T> collection)
		{
			int num = collection.Count<T>();
			if (num == 0)
			{
				return default(T);
			}
			return collection.ElementAt(Random.Range(0, num));
		}

		// Token: 0x06000108 RID: 264 RVA: 0x00006BE6 File Offset: 0x00004DE6
		public static float Snap(this float value, float increment)
		{
			return Mathf.Round(value / increment) * increment;
		}

		// Token: 0x06000109 RID: 265 RVA: 0x00006BF2 File Offset: 0x00004DF2
		public static Vector2 Snap(this Vector2 value, float increment)
		{
			return new Vector2(value.x.Snap(increment), value.y.Snap(increment));
		}

		// Token: 0x0600010A RID: 266 RVA: 0x00006C11 File Offset: 0x00004E11
		public static Vector3 Snap(this Vector3 value, float increment)
		{
			return new Vector3(value.x.Snap(increment), value.y.Snap(increment), value.z.Snap(increment));
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00006C3C File Offset: 0x00004E3C
		public static float ToPercentFloat(this float value)
		{
			return value * 100f;
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00006C45 File Offset: 0x00004E45
		public static int ToPercentInt(this float value)
		{
			return Mathf.RoundToInt(value.ToPercentFloat());
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00006C52 File Offset: 0x00004E52
		public static string ToPercent(this int value)
		{
			return value.ToString() + "%";
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00006C68 File Offset: 0x00004E68
		public static string ToPercent(this float value)
		{
			return value.ToPercentInt().ToString() + "%";
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00006C8D File Offset: 0x00004E8D
		public static string ToPercent(this byte value)
		{
			return value.ToString() + "%";
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00006CA0 File Offset: 0x00004EA0
		public static string ToPercentColored(this float value)
		{
			return ((value >= 0f) ? "<color=#00ff00>" : "<color=#ff0000>") + value.ToPercent() + "</color>";
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00006CC8 File Offset: 0x00004EC8
		public static List<T> ToEntityList<T>(this IEnumerable<string> entityNames) where T : ShEntity
		{
			List<T> list = new List<T>();
			foreach (string text in entityNames)
			{
				T item;
				if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<T>(text, out item))
				{
					list.Add(item);
				}
				else
				{
					Util.Log("Entity " + text + " not found", LogLevel.Warn);
				}
			}
			return list;
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00006D40 File Offset: 0x00004F40
		public static void CopyToClipboard(this string str)
		{
			TextEditor textEditor = new TextEditor();
			textEditor.text = str;
			textEditor.SelectAll();
			textEditor.Copy();
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000113 RID: 275 RVA: 0x00006D59 File Offset: 0x00004F59
		public static DateTimeOffset CurrentTime
		{
			get
			{
				return DateTimeOffset.UtcNow;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000114 RID: 276 RVA: 0x00006D60 File Offset: 0x00004F60
		public static string FormattedTime
		{
			get
			{
				return DateTimeOffset.UtcNow.ToString("d/M/yy HH:mm");
			}
		}

		// Token: 0x06000115 RID: 277 RVA: 0x00006D7F File Offset: 0x00004F7F
		public static string FormatTime(this DateTimeOffset dateTime)
		{
			return dateTime.ToString("d/M/yy HH:mm");
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00006D90 File Offset: 0x00004F90
		public static string TimeStringFromSeconds(this float pastTime)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)pastTime);
			StringBuilder stringBuilder = new StringBuilder();
			if (timeSpan.Days > 0)
			{
				stringBuilder.Append(timeSpan.Days.ToString()).Append("D ");
			}
			if (timeSpan.Days > 0 || timeSpan.Hours > 0)
			{
				stringBuilder.Append(timeSpan.Hours.ToString()).Append("H ");
			}
			stringBuilder.Append(timeSpan.Minutes.ToString("D2")).Append(":").Append(timeSpan.Seconds.ToString("D2"));
			return stringBuilder.ToString();
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00006E4D File Offset: 0x0000504D
		public static bool SafePosition(this Vector3 p, out RaycastHit hit, float heightCheck = 5f)
		{
			return Physics.Raycast(p + Vector3.up * heightCheck, Vector3.down, out hit, heightCheck * 2f, 9217);
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00006E77 File Offset: 0x00005077
		public static Quaternion SafeLookRotation(this Vector3 v, Vector3 up)
		{
			if (!(v == Vector3.zero))
			{
				return Quaternion.LookRotation(v, up);
			}
			return Quaternion.identity;
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00006E94 File Offset: 0x00005094
		public static float Pitch(this Quaternion q)
		{
			return Mathf.Atan2(2f * q.x * q.w - 2f * q.y * q.z, 1f - 2f * q.x * q.x - 2f * q.z * q.z);
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00006EFA File Offset: 0x000050FA
		public static float Roll(this Quaternion q)
		{
			return (2f * q.x * q.y + 2f * q.z * q.w).FastASin();
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00006F28 File Offset: 0x00005128
		public static Int3 ToTileSpace(this Vector3 worldSpace, int x, int z, RecastGraph recastGraph)
		{
			worldSpace -= new Vector3((float)x * recastGraph.TileWorldSizeX, 0f, (float)z * recastGraph.TileWorldSizeZ);
			return (Int3)worldSpace;
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00006F54 File Offset: 0x00005154
		public static int SubgridToGrid(int subgridIndex, Vector2Int subMin, Vector2Int subMax, int mainSizeX)
		{
			int num = subMax.x - subMin.x + 1;
			return (subgridIndex / num + subMin.y) * mainSizeX + (subgridIndex % num + subMin.x);
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00006F8C File Offset: 0x0000518C
		public static int GetGroundNodesInArea(RecastGraph graph, Bounds area, ref ValueTuple<int, int, int>[] nodes)
		{
			int num = nodes.Length;
			IntRect touchingTiles = graph.GetTouchingTiles(area, 0f);
			int area2 = touchingTiles.Area;
			int num2 = 0;
			int num3 = 0;
			for (int i = touchingTiles.xmin; i <= touchingTiles.xmax; i++)
			{
				for (int j = touchingTiles.ymin; j <= touchingTiles.ymax; j++)
				{
					num2++;
					TriangleMeshNode[] nodes2 = graph.GetTile(i, j).nodes;
					for (int k = 0; k < nodes2.Length; k++)
					{
						TriangleMeshNode triangleMeshNode = nodes2[k];
						if (triangleMeshNode.Tag == 0U && area.Contains((Vector3)triangleMeshNode.position))
						{
							nodes[num3] = new ValueTuple<int, int, int>(i, j, k);
							if (++num3 >= num)
							{
								return num3;
							}
							if (num3 >= num * num2 / area2)
							{
								break;
							}
						}
					}
				}
			}
			return num3;
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00007070 File Offset: 0x00005270
		public static List<int> GetTileNodesInArea(NavmeshTile tile, Bounds area)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < tile.nodes.Length; i++)
			{
				if (area.Contains((Vector3)tile.nodes[i].position))
				{
					list.Add(i);
				}
			}
			return list;
		}

		// Token: 0x0600011F RID: 287 RVA: 0x000070B9 File Offset: 0x000052B9
		public static void RotateWheel(this Transform wheelT, float speed, float radius)
		{
			wheelT.Rotate(Vector3.right, speed / radius * Time.deltaTime * 57.29578f);
		}

		// Token: 0x06000120 RID: 288 RVA: 0x000070D5 File Offset: 0x000052D5
		public static void Send(this Peer peer, ref Packet packet)
		{
			peer.Send(0, ref packet);
		}

		// Token: 0x06000121 RID: 289 RVA: 0x000070E1 File Offset: 0x000052E1
		public static float RTT(this Peer peer)
		{
			return peer.LastRoundTripTime * 0.001f;
		}

		// Token: 0x06000122 RID: 290 RVA: 0x000070F2 File Offset: 0x000052F2
		public static string PrettyRTT(this Peer peer)
		{
			return string.Format("{0} ms", Mathf.RoundToInt(peer.RTT() * 1000f));
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00007114 File Offset: 0x00005314
		public static string PrettyFPS()
		{
			return Mathf.RoundToInt(1f / Time.unscaledDeltaTime).ToString();
		}

		// Token: 0x06000124 RID: 292 RVA: 0x0000713C File Offset: 0x0000533C
		public static byte[] LoadFile(string filePath)
		{
			byte[] result;
			try
			{
				result = File.ReadAllBytes(filePath);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000125 RID: 293 RVA: 0x00007168 File Offset: 0x00005368
		public static Texture2D LoadImage(byte[] data)
		{
			Texture2D result;
			try
			{
				Texture2D texture2D = new Texture2D(1, 1);
				texture2D.LoadImage(data);
				result = texture2D;
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000126 RID: 294 RVA: 0x000071A0 File Offset: 0x000053A0
		public static Texture2D LoadImageFile(string filePath)
		{
			Texture2D result;
			try
			{
				result = Util.LoadImage(Util.LoadFile(filePath));
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000127 RID: 295 RVA: 0x000071D4 File Offset: 0x000053D4
		public static T RandomEnumValue<T>()
		{
			Array values = Enum.GetValues(typeof(T));
			return (T)((object)values.GetValue(Random.Range(0, values.Length)));
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00007208 File Offset: 0x00005408
		public static void RebuildLayout(this MonoBehaviour b)
		{
			b.StartCoroutine(Util.RebuildLayout(b.GetComponent<RectTransform>()));
		}

		// Token: 0x06000129 RID: 297 RVA: 0x0000721C File Offset: 0x0000541C
		private static IEnumerator RebuildLayout(RectTransform rect)
		{
			while (rect)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
				yield return null;
			}
			yield break;
		}

		// Token: 0x0600012A RID: 298 RVA: 0x0000722B File Offset: 0x0000542B
		public static Color GetJobColor(IList<JobInfoShared> sharedJobs, int jobIndex, float alpha = 1f)
		{
			if (jobIndex >= 0 && jobIndex < sharedJobs.Count)
			{
				return sharedJobs[jobIndex].GetColor(alpha);
			}
			return new Color(1f, 1f, 1f, alpha);
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00007260 File Offset: 0x00005460
		public static bool GetCommandArgument(string key, out string value)
		{
			key = key.ToLower();
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			int num = commandLineArgs.Length;
			for (int i = 0; i < num; i++)
			{
				if (commandLineArgs[i].ToLower() == key)
				{
					i++;
					if (i < num)
					{
						value = commandLineArgs[i];
					}
					else
					{
						value = null;
					}
					return true;
				}
			}
			value = null;
			return false;
		}

		// Token: 0x0600012C RID: 300 RVA: 0x000072B4 File Offset: 0x000054B4
		public static void Log(string message, LogLevel level = LogLevel.Log)
		{
			switch (level)
			{
			case LogLevel.Log:
				Debug.Log("[LOG] " + message);
				return;
			case LogLevel.Warn:
				Debug.LogWarning("[WRN] " + message);
				return;
			case LogLevel.Error:
				Debug.LogError("[ERR] " + message);
				return;
			default:
				return;
			}
		}

		// Token: 0x0600012D RID: 301 RVA: 0x00007306 File Offset: 0x00005506
		public static void PlayRandomPitch(this AudioSource audioSource, float offset = 0.05f)
		{
			audioSource.pitch = Random.Range(1f - offset, 1f + offset);
			audioSource.Play();
		}

		// Token: 0x0600012E RID: 302 RVA: 0x00007327 File Offset: 0x00005527
		public static bool InWater(Vector3 position)
		{
			return position.y < MonoBehaviourSingleton<SceneManager>.Instance.WaterLevel(position);
		}

		// Token: 0x0600012F RID: 303 RVA: 0x0000733C File Offset: 0x0000553C
		public static void DoBuoyancy(Vector3 position, Rigidbody rb)
		{
			float a = position.y - MonoBehaviourSingleton<SceneManager>.Instance.WaterLevel(position);
			rb.AddForceAtPosition(Mathf.Max(a, -2f) * Physics.gravity, rb.worldCenterOfMass, ForceMode.Acceleration);
		}

		// Token: 0x06000130 RID: 304 RVA: 0x00007380 File Offset: 0x00005580
		public static List<Type> GetAllSubclasses(Type parentType)
		{
			List<Type> list = (from type in Assembly.GetAssembly(parentType).GetTypes()
			where type.IsSubclassOf(parentType)
			select type).ToList<Type>();
			list.Insert(0, parentType);
			return list;
		}

		// Token: 0x06000131 RID: 305 RVA: 0x000073CD File Offset: 0x000055CD
		public static Color ToRandomColor(this string input)
		{
			Random.InitState(input.GetHashCode());
			return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
		}

		// Token: 0x06000132 RID: 306 RVA: 0x0000740C File Offset: 0x0000560C
		public static void ScrollBottom(this ScrollView scrollView)
		{
			scrollView.scrollOffset = Vector2.one * float.PositiveInfinity;
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00007423 File Offset: 0x00005623
		public static void HitWorld(RaycastHit hit)
		{
			(hit.collider.CompareTag("Grass") ? Util.hitDirtBuffer : Util.hitBuffer).Execute(hit.point, hit.normal);
		}

		// Token: 0x06000134 RID: 308 RVA: 0x00007458 File Offset: 0x00005658
		public static string LogTransform(this Transform t)
		{
			return string.Format("Pos: {0} Place: {1}", t.position, t.parent.GetSiblingIndex());
		}

		// Token: 0x06000135 RID: 309 RVA: 0x00007480 File Offset: 0x00005680
		public static void SetVisibility(this GameObject g, bool setting)
		{
			Renderer[] componentsInChildren = g.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = setting;
			}
		}

		// Token: 0x0400028B RID: 651
		public const int depthBufferSize = 24;

		// Token: 0x0400028C RID: 652
		public const string navVersion = "NAV21";

		// Token: 0x0400028D RID: 653
		public const int safetyDelayMS = 1000;

		// Token: 0x0400028E RID: 654
		public const float serverFudge = 0.75f;

		// Token: 0x0400028F RID: 655
		public const long timeSliceMS = 15L;

		// Token: 0x04000290 RID: 656
		public const float netFrameTime = 0.1f;

		// Token: 0x04000291 RID: 657
		public const int standardFramerate = 72;

		// Token: 0x04000292 RID: 658
		public const string localHost = "127.0.0.1";

		// Token: 0x04000293 RID: 659
		public const char addressDelimiter = ':';

		// Token: 0x04000294 RID: 660
		public const ushort offlinePort = 5557;

		// Token: 0x04000295 RID: 661
		public const string batchmodeArgument = "-batchmode";

		// Token: 0x04000296 RID: 662
		public const string singleplayerArgument = "-singleplayer";

		// Token: 0x04000297 RID: 663
		public const string connectArgument = "-connect";

		// Token: 0x04000298 RID: 664
		public const string mapArgument = "-map";

		// Token: 0x04000299 RID: 665
		public const string ignorePluginsArgument = "-ignorePlugins";

		// Token: 0x0400029A RID: 666
		public const float respondTime = 10f;

		// Token: 0x0400029B RID: 667
		public const string defaultID = "Default";

		// Token: 0x0400029C RID: 668
		public const int defaultTextSize = 44;

		// Token: 0x0400029D RID: 669
		public const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		// Token: 0x0400029E RID: 670
		public static readonly Dictionary<string, Type> baseReferences = new Dictionary<string, Type>
		{
			{
				"go",
				null
			},
			{
				"mainT",
				typeof(Transform)
			}
		};

		// Token: 0x0400029F RID: 671
		public const int sampleRate = 16000;

		// Token: 0x040002A0 RID: 672
		public static readonly Color[] healthColors = new Color[]
		{
			new Color(0.6f, 0f, 0f),
			new Color(0.7f, 0.25f, 0f),
			new Color(0f, 0.05f, 0.57f),
			new Color(0.63f, 0.61f, 0f)
		};

		// Token: 0x040002A1 RID: 673
		public static readonly ContactPoint[] contactBuffer = new ContactPoint[8];

		// Token: 0x040002A2 RID: 674
		public static readonly Collider[] colliderBuffer = new Collider[64];

		// Token: 0x040002A3 RID: 675
		public static readonly int[] waypointGraphIndex = new int[]
		{
			0,
			1,
			-1,
			2,
			-1,
			-1
		};

		// Token: 0x040002A4 RID: 676
		public const string referencePrefabField = "referencePrefab";

		// Token: 0x040002A5 RID: 677
		public static readonly byte[] fourBytes = new byte[4];

		// Token: 0x040002A6 RID: 678
		public const float hundredF = 100f;

		// Token: 0x040002A7 RID: 679
		public const byte hundredB = 100;

		// Token: 0x040002A8 RID: 680
		public const int maxPing = 1000;

		// Token: 0x040002A9 RID: 681
		public const int appLimit = 30;

		// Token: 0x040002AA RID: 682
		public const int maxPacketLength = 1362;

		// Token: 0x040002AB RID: 683
		public const string scrollContainer = "unity-content-container";

		// Token: 0x040002AC RID: 684
		private const string dateTimeFormat = "d/M/yy HH:mm";

		// Token: 0x040002AD RID: 685
		public const float collisionDamageMinimum = 24f;

		// Token: 0x040002AE RID: 686
		public const float maxCameraAngle = 25f;

		// Token: 0x040002AF RID: 687
		private const int chatLength = 64;

		// Token: 0x040002B0 RID: 688
		private const int profileLength = 128;

		// Token: 0x040002B1 RID: 689
		private const int divisions = 128;

		// Token: 0x040002B2 RID: 690
		public const float PI2 = 6.2831855f;

		// Token: 0x040002B3 RID: 691
		private static readonly float[] sinArray = new float[129];

		// Token: 0x040002B4 RID: 692
		private static readonly float[] cosArray = new float[129];

		// Token: 0x040002B5 RID: 693
		private static readonly float[] aSinArray = new float[129];

		// Token: 0x040002B6 RID: 694
		private static readonly float[] aCosArray = new float[129];

		// Token: 0x040002B7 RID: 695
		public static readonly Vector2[] randomVector = new Vector2[128];

		// Token: 0x040002B8 RID: 696
		public static readonly byte[] inventoryElement = new byte[8];

		// Token: 0x040002B9 RID: 697
		public static readonly byte[] shopElement = new byte[12];

		// Token: 0x040002BA RID: 698
		public static readonly Quaternion badRotation = new Quaternion(0f, 0f, 0f, -1f);

		// Token: 0x040002BB RID: 699
		public const float syncInterval = 5f;

		// Token: 0x040002BC RID: 700
		public const float slowInput = 0.5f;

		// Token: 0x040002BD RID: 701
		public const float fastInput = 1.25f;

		// Token: 0x040002BE RID: 702
		public const float forceScalar = 0.75f;

		// Token: 0x040002BF RID: 703
		public const float switchTime = 1f;

		// Token: 0x040002C0 RID: 704
		public const char commandPrefix = '/';

		// Token: 0x040002C1 RID: 705
		public const char stringDelimiter = '"';

		// Token: 0x040002C2 RID: 706
		public const char colorDelimiter = '&';

		// Token: 0x040002C3 RID: 707
		public static readonly char[] commandSeparators = new char[]
		{
			' '
		};

		// Token: 0x040002C4 RID: 708
		public static readonly HashSet<char> badMessageChars = new HashSet<char>
		{
			'<',
			'\n',
			'\r',
			'&'
		};

		// Token: 0x040002C5 RID: 709
		public static readonly HashSet<char> badCredentialChars = new HashSet<char>
		{
			'<',
			'\n',
			'\r',
			'&',
			'"',
			'/'
		};

		// Token: 0x040002C6 RID: 710
		public static readonly char[] colorSeparators = new char[]
		{
			'&'
		};

		// Token: 0x040002C7 RID: 711
		public static readonly PermEnum[] permArray = (PermEnum[])Enum.GetValues(typeof(PermEnum));

		// Token: 0x040002C8 RID: 712
		public static readonly Dictionary<char, string> colorValues = new Dictionary<char, string>
		{
			{
				'0',
				"000000"
			},
			{
				'1',
				"0000aa"
			},
			{
				'2',
				"00aa00"
			},
			{
				'3',
				"00aaaa"
			},
			{
				'4',
				"aa0000"
			},
			{
				'5',
				"aa00aa"
			},
			{
				'6',
				"ffaa00"
			},
			{
				'7',
				"aaaaaa"
			},
			{
				'8',
				"555555"
			},
			{
				'9',
				"5555ff"
			},
			{
				'a',
				"55ff55"
			},
			{
				'b',
				"55ffff"
			},
			{
				'c',
				"ff5555"
			},
			{
				'd',
				"ff55ff"
			},
			{
				'e',
				"ffff55"
			},
			{
				'f',
				"ffffff"
			}
		};

		// Token: 0x040002C9 RID: 713
		public static readonly Language[] languages = new Language[]
		{
			new Language("EN", "English", "English"),
			new Language("PL", "Polish", "Polskie"),
			new Language("RU", "Russian", "русский"),
			new Language("ES", "Spanish", "Español"),
			new Language("PT", "Portuguese", "Portugues"),
			new Language("TR", "Turkish", "Türkçe"),
			new Language("FR", "French", "Français"),
			new Language("DE", "German", "Deutsche"),
			new Language("TH", "Thai", "ไทย"),
			new Language("CS", "Czech", "Čeština"),
			new Language("AZ", "Azerbaijani", "Azərbaycan"),
			new Language("UK", "Ukrainian", "украї́нська"),
			new Language("ID", "Indonesian", "Bahasa Indonesia")
		};

		// Token: 0x040002CA RID: 714
		public const float maxLatency = 0.2f;

		// Token: 0x040002CB RID: 715
		public const float processDuration = 2f;

		// Token: 0x040002CC RID: 716
		public const float sendRate = 10f;

		// Token: 0x040002CD RID: 717
		public const float sendInterval = 0.1f;

		// Token: 0x040002CE RID: 718
		public const float useDistance = 8f;

		// Token: 0x040002CF RID: 719
		public const float inviteDistance = 30f;

		// Token: 0x040002D0 RID: 720
		public const float inviteDistanceSqr = 900f;

		// Token: 0x040002D1 RID: 721
		public const float animateSpeedSqr = 0.1f;

		// Token: 0x040002D2 RID: 722
		public const float closeDist2 = 1f;

		// Token: 0x040002D3 RID: 723
		public const float fastTime = 0.1f;

		// Token: 0x040002D4 RID: 724
		public const float staticDeltaV2 = 1f;

		// Token: 0x040002D5 RID: 725
		public const float sectorSize = 32f;

		// Token: 0x040002D6 RID: 726
		public const int netSectorRange = 8;

		// Token: 0x040002D7 RID: 727
		public const float netPerpendicularRange = 256f;

		// Token: 0x040002D8 RID: 728
		public const float netVisibleRange = 362.03867f;

		// Token: 0x040002D9 RID: 729
		public const float netVisibleRangeSqr = 131071.99f;

		// Token: 0x040002DA RID: 730
		public const float closeRangeSqr = 25f;

		// Token: 0x040002DB RID: 731
		public const float pathfindRange = 12f;

		// Token: 0x040002DC RID: 732
		public const float pathfindRangeSqr = 144f;

		// Token: 0x040002DD RID: 733
		public const float normalDrag = 0.1f;

		// Token: 0x040002DE RID: 734
		public const float waterDrag = 2f;

		// Token: 0x040002DF RID: 735
		[NonSerialized]
		public static IdentityBuffer identityBuffer;

		// Token: 0x040002E0 RID: 736
		[NonSerialized]
		public static FollowBuffer smokeTrailBuffer;

		// Token: 0x040002E1 RID: 737
		[NonSerialized]
		public static FollowBuffer fireEffectBuffer;

		// Token: 0x040002E2 RID: 738
		[NonSerialized]
		public static LightningBuffer lightningEffectBuffer;

		// Token: 0x040002E3 RID: 739
		[NonSerialized]
		public static TimedBuffer hitBuffer;

		// Token: 0x040002E4 RID: 740
		[NonSerialized]
		public static TimedBuffer hitDirtBuffer;

		// Token: 0x040002E5 RID: 741
		[NonSerialized]
		public static TracerBuffer tracerBuffer;

		// Token: 0x040002E6 RID: 742
		[NonSerialized]
		public static TrailBuffer hitscanTrailBuffer;

		// Token: 0x040002E7 RID: 743
		[NonSerialized]
		public static LaserBuffer laserBuffer;

		// Token: 0x040002E8 RID: 744
		[NonSerialized]
		public static BaseBuffer skidMarkBuffer;

		// Token: 0x040002E9 RID: 745
		[NonSerialized]
		public static CanvasBuffer damageMarkerBuffer;

		// Token: 0x040002EA RID: 746
		[NonSerialized]
		public static TimedBuffer[] hitEffects;

		// Token: 0x040002EB RID: 747
		[NonSerialized]
		public static FollowBuffer[] fireEffects;

		// Token: 0x040002EC RID: 748
		[NonSerialized]
		public static TimedBuffer[] destroyEffects;

		// Token: 0x040002ED RID: 749
		[NonSerialized]
		public static FollowBuffer[] thrownEffects;

		// Token: 0x040002EE RID: 750
		public const float SQRT2 = 1.4142135f;

		// Token: 0x040002EF RID: 751
		public const float SQRT3 = 1.7320508f;

		// Token: 0x040002F0 RID: 752
		public static List<Type> itemTypes;
	}
}
