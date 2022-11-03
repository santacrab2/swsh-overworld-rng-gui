using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SysBot.Base;
using PKHeX.Core;
using System.Collections;
using System.Threading.Tasks;

namespace SWSH_OWRNG_Generator.WinForms
{
    public partial class OverworldSpawnViewer : Form
    {
        public static CancellationToken token = new();
        public static MainWindow mainWindow;
        private readonly static SwitchConnectionConfig Config = new() { Protocol = SwitchProtocol.WiFi, IP = Properties.Settings.Default.SwitchIP, Port = 6000 };
        public SwitchSocketAsync SwitchConnection = new(Config);
        public OverworldSpawnViewer(MainWindow f)
        {
            mainWindow = f;
            InitializeComponent();
            
        }

     

        public async void ReadSpawns_Click(object sender, EventArgs e)
        {
            SwitchConnection.Connect();
            var kcoordinates = await SwitchConnection.ReadBytesAsync(0x4505B3C0, 24592,token);
           
            var OverWorldSpawnList = await ReadOwPokemonFromBlock(kcoordinates,MainWindow.sav, token);
            List<OWSpawnInfo> OWSIlist = new();
            foreach(var pk in OverWorldSpawnList)
            {
                var themark = MainWindow.HasMark(pk, out var mark);
                var spawn = new OWSpawnInfo() { species = (Species)pk.Species, ability = (Ability)pk.Ability, EC = $"{pk.EncryptionConstant:X8}", gender = (Gender)pk.Gender, Atk=pk.IV_ATK,HP = pk.IV_HP,Def = pk.IV_DEF,SpA = pk.IV_SPA,SpD = pk.IV_SPD, Spe = pk.IV_SPE, nature = (Nature)pk.Nature, PID = $"{pk.PID:X8}", shiny = pk.IsShiny, mark = themark? mark : null };
                OWSIlist.Add(spawn);
            }
            BindingSource spawnsource = new() { DataSource = OWSIlist };
            OverworldSpawnResults.DataSource = spawnsource;
            spawnsource.ResetBindings(false);
            SwitchConnection.Disconnect();
        }
        public async Task<List<PK8>> ReadOwPokemonFromBlock(byte[] KCoordinates, SAV8SWSH sav, CancellationToken token)
        {
            var PK8s = new List<PK8>();

            var i = 8;
            var j = 0;
            var count = 0;
            var last_index = i;

            while (!token.IsCancellationRequested && i < KCoordinates.Length)
            {
                if (j == 12)
                {
                    if (KCoordinates[i - 68] != 0 && KCoordinates[i - 68] != 255)
                    {
                        var bytes = KCoordinates.Slice(i - 68, 56);
                        j = 0;
                        i = last_index + 8;
                        last_index = i;
                        count++;
                        var pkm = await ReadOwPokemon(0, 0, bytes, sav, token).ConfigureAwait(false);
                        if (pkm != null)
                            PK8s.Add(pkm);
                    }
                }

                if (KCoordinates[i] == 0xFF)
                {
                    if (i % 8 == 0)
                        last_index = i;
                    i++;
                    j++;
                }
                else
                {
                    j = 0;
                    if (i == last_index)
                    {
                        i += 8;
                        last_index = i;
                    }
                    else
                    {
                        i = last_index + 8;
                        last_index = i;
                    }
                }

            }
            return PK8s;
        }
        public async Task<PK8?> ReadOwPokemon(Species target, uint startoffset, byte[]? mondata, SAV8SWSH TrainerData, CancellationToken token)
        {
            byte[]? data = null;
            Species species = 0;
            uint offset = startoffset;
            int i = 0;

            if (target != (Species)0)
            {
                do
                {
                    data = await SwitchConnection.ReadBytesAsync(offset, 56, token).ConfigureAwait(false);
                    species = (Species)BitConverter.ToUInt16(data.Slice(0, 2), 0);
                    offset += 192;
                    i++;
                } while (target != 0 && species != 0 && target != species && i <= 40);
                if (i > 40)
                    data = null;
            }
            else if (mondata != null)
            {
                data = mondata;
                species = (Species)BitConverter.ToUInt16(data.Slice(0, 2), 0);
            }

            if (data != null && data[20] == 1)
            {
                var pk = new PK8
                {
                    Species = (ushort)species,
                    Form = data[2],
                    CurrentLevel = data[4],
                    Met_Level = data[4],
                    Nature = data[8],
                    Gender = (data[10] == 1) ? 0 : 1,
                    OT_Name = TrainerData.OT,
                    TID = TrainerData.TID,
                    SID = TrainerData.SID,
                    OT_Gender = TrainerData.Gender,
                    HT_Name = TrainerData.OT,
                    HT_Gender = TrainerData.Gender,
                    Move1 = BitConverter.ToUInt16(data.Slice(48, 2), 0),
                    Move2 = BitConverter.ToUInt16(data.Slice(50, 2), 0),
                    Move3 = BitConverter.ToUInt16(data.Slice(52, 2), 0),
                    Move4 = BitConverter.ToUInt16(data.Slice(54, 2), 0),
                    Version = 44,
                };
                pk.SetNature(data[8]);
                pk.SetAbility(data[12] - 1);
                if (data[22] != 255)
                    pk.SetRibbonIndex((RibbonIndex)data[22]);
                if (!pk.IsGenderValid())
                    pk.Gender = 2;
                if (data[14] == 1)
                    pk.HeldItem = data[16];

                FakeShiny shinyness = (FakeShiny)(data[6] + 1);
                int ivs = data[18];
                uint seed = BitConverter.ToUInt32(data.Slice(24, 4), 0);

                pk = CalculateFromSeed(pk, shinyness, ivs, seed);

                return pk;
            }
            else
                return null;
        }
        public static PK8 CalculateFromSeed(PK8 pk, FakeShiny shiny, int flawless, uint seed)
        {
            var UNSET = -1;
            var xoro = new Xoroshiro128Plus(seed);

            // Encryption Constant
            pk.EncryptionConstant = (uint)xoro.NextInt(uint.MaxValue);

            // PID
            var pid = (uint)xoro.NextInt(uint.MaxValue);
            if (shiny == FakeShiny.Never)
            {
                if (GetIsShiny(pk.TID, pk.SID, pid))
                    pid ^= 0x1000_0000;
            }

            else if (shiny != FakeShiny.Random)
            {
                if (!GetIsShiny(pk.TID, pk.SID, pid))
                    pid = GetShinyPID(pk.TID, pk.SID, pid, 0);
            }

            pk.PID = pid;

            // IVs
            var ivs = new[] { UNSET, UNSET, UNSET, UNSET, UNSET, UNSET };
            const int MAX = 31;
            for (int i = 0; i < flawless; i++)
            {
                int index;
                do { index = (int)xoro.NextInt(6); }
                while (ivs[index] != UNSET);

                ivs[index] = MAX;
            }

            for (int i = 0; i < ivs.Length; i++)
            {
                if (ivs[i] == UNSET)
                    ivs[i] = (int)xoro.NextInt(32);
            }

            pk.IV_HP = ivs[0];
            pk.IV_ATK = ivs[1];
            pk.IV_DEF = ivs[2];
            pk.IV_SPA = ivs[3];
            pk.IV_SPD = ivs[4];
            pk.IV_SPE = ivs[5];

            return pk;
        }

        private static uint GetShinyPID(int tid, int sid, uint pid, int type)
        {
            return (uint)(((tid ^ sid ^ (pid & 0xFFFF) ^ type) << 16) | (pid & 0xFFFF));
        }

        private static bool GetIsShiny(int tid, int sid, uint pid)
        {
            return GetShinyXor(pid, (uint)((sid << 16) | tid)) < 16;
        }

        private static uint GetShinyXor(uint pid, uint oid)
        {
            var xor = pid ^ oid;
            return (xor ^ (xor >> 16)) & 0xFFFF;
        }
        public enum FakeShiny : byte
        {
            /// <summary>
            /// PID is fixed to a specified value.
            /// </summary>
            FixedValue = 0,

            /// <summary>
            /// PID is purely random; can be shiny or not shiny.
            /// </summary>
            Random = 1,

            /// <summary>
            /// PID is randomly created and forced to be shiny.
            /// </summary>
            Always = 2,

            /// <summary>
            /// PID is randomly created and forced to be not shiny.
            /// </summary>
            Never = 3,

            /// <summary>
            /// PID is randomly created and forced to be shiny as Stars.
            /// </summary>
            AlwaysStar = 5,

            /// <summary>
            /// PID is randomly created and forced to be shiny as Squares.
            /// </summary>
            AlwaysSquare = 6,
        }

      
    }
    public class OWSpawnInfo
    {
        public Species species { get; set; }
        public Gender gender { get; set; }
        public bool shiny { get; set; }
        public RibbonIndex? mark { get; set; }
        public string PID { get; set; }
        public string EC { get; set; }
        public Nature nature { get; set; }
        public Ability ability { get; set; }
        public int HP { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int SpA { get; set; }
        public int SpD { get; set; }
        public int Spe { get; set; }
    }

}
