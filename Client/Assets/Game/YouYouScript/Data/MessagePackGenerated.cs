// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Resolvers
{
    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        private GeneratedResolver()
        {
        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            internal static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> Formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    Formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        private static readonly global::System.Collections.Generic.Dictionary<global::System.Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<global::System.Type, int>(4)
            {
                { typeof(global::System.Collections.Generic.Dictionary<int, int>), 0 },
                { typeof(global::System.Collections.Generic.List<float>), 1 },
                { typeof(global::DataManager), 2 },
                { typeof(global::PlayerRoleData), 3 },
            };
        }

        internal static object GetFormatter(global::System.Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key))
            {
                return null;
            }

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.DictionaryFormatter<int, int>();
                case 1: return new global::MessagePack.Formatters.ListFormatter<float>();
                case 2: return new MessagePack.Formatters.DataManagerFormatter();
                case 3: return new MessagePack.Formatters.PlayerRoleDataFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1649 // File name should match first type name




// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1129 // Do not use default value type constructor
#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Formatters
{
    public sealed class DataManagerFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::DataManager>
    {
        // UserId
        private static global::System.ReadOnlySpan<byte> GetSpan_UserId() => new byte[1 + 6] { 166, 85, 115, 101, 114, 73, 100 };
        // IsFirstLoginTime
        private static global::System.ReadOnlySpan<byte> GetSpan_IsFirstLoginTime() => new byte[1 + 16] { 176, 73, 115, 70, 105, 114, 115, 116, 76, 111, 103, 105, 110, 84, 105, 109, 101 };
        // DataUpdateTime
        private static global::System.ReadOnlySpan<byte> GetSpan_DataUpdateTime() => new byte[1 + 14] { 174, 68, 97, 116, 97, 85, 112, 100, 97, 116, 101, 84, 105, 109, 101 };
        // PlayerRoleData
        private static global::System.ReadOnlySpan<byte> GetSpan_PlayerRoleData() => new byte[1 + 14] { 174, 80, 108, 97, 121, 101, 114, 82, 111, 108, 101, 68, 97, 116, 97 };

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::DataManager value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNil();
                return;
            }

            var formatterResolver = options.Resolver;
            writer.WriteMapHeader(4);
            writer.WriteRaw(GetSpan_UserId());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Serialize(ref writer, value.UserId, options);
            writer.WriteRaw(GetSpan_IsFirstLoginTime());
            writer.Write(value.IsFirstLoginTime);
            writer.WriteRaw(GetSpan_DataUpdateTime());
            writer.Write(value.DataUpdateTime);
            writer.WriteRaw(GetSpan_PlayerRoleData());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::PlayerRoleData>(formatterResolver).Serialize(ref writer, value.PlayerRoleData, options);
        }

        public global::DataManager Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            var formatterResolver = options.Resolver;
            var length = reader.ReadMapHeader();
            var ____result = new global::DataManager();

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.Internal.CodeGenHelpers.ReadStringSpan(ref reader);
                switch (stringKey.Length)
                {
                    default:
                    FAIL:
                      reader.Skip();
                      continue;
                    case 6:
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 110266614641493UL) { goto FAIL; }

                        ____result.UserId = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Deserialize(ref reader, options);
                        continue;
                    case 16:
                        if (!global::System.MemoryExtensions.SequenceEqual(stringKey, GetSpan_IsFirstLoginTime().Slice(1))) { goto FAIL; }

                        ____result.IsFirstLoginTime = reader.ReadBoolean();
                        continue;
                    case 14:
                        switch (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey))
                        {
                            default: goto FAIL;
                            case 7017857631359623492UL:
                                if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 111520592979316UL) { goto FAIL; }

                                ____result.DataUpdateTime = reader.ReadInt32();
                                continue;

                            case 8021599666453965904UL:
                                if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 107152475972972UL) { goto FAIL; }

                                ____result.PlayerRoleData = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::PlayerRoleData>(formatterResolver).Deserialize(ref reader, options);
                                continue;

                        }

                }
            }

            reader.Depth--;
            return ____result;
        }
    }

    public sealed class PlayerRoleDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::PlayerRoleData>
    {
        // playerBornPos
        private static global::System.ReadOnlySpan<byte> GetSpan_playerBornPos() => new byte[1 + 13] { 173, 112, 108, 97, 121, 101, 114, 66, 111, 114, 110, 80, 111, 115 };
        // playerPos
        private static global::System.ReadOnlySpan<byte> GetSpan_playerPos() => new byte[1 + 9] { 169, 112, 108, 97, 121, 101, 114, 80, 111, 115 };
        // playerRotate
        private static global::System.ReadOnlySpan<byte> GetSpan_playerRotate() => new byte[1 + 12] { 172, 112, 108, 97, 121, 101, 114, 82, 111, 116, 97, 116, 101 };
        // cameraRotate
        private static global::System.ReadOnlySpan<byte> GetSpan_cameraRotate() => new byte[1 + 12] { 172, 99, 97, 109, 101, 114, 97, 82, 111, 116, 97, 116, 101 };
        // firstEntryLevel
        private static global::System.ReadOnlySpan<byte> GetSpan_firstEntryLevel() => new byte[1 + 15] { 175, 102, 105, 114, 115, 116, 69, 110, 116, 114, 121, 76, 101, 118, 101, 108 };
        // levelId
        private static global::System.ReadOnlySpan<byte> GetSpan_levelId() => new byte[1 + 7] { 167, 108, 101, 118, 101, 108, 73, 100 };
        // curExp
        private static global::System.ReadOnlySpan<byte> GetSpan_curExp() => new byte[1 + 6] { 166, 99, 117, 114, 69, 120, 112 };
        // totalExp
        private static global::System.ReadOnlySpan<byte> GetSpan_totalExp() => new byte[1 + 8] { 168, 116, 111, 116, 97, 108, 69, 120, 112 };
        // curHp
        private static global::System.ReadOnlySpan<byte> GetSpan_curHp() => new byte[1 + 5] { 165, 99, 117, 114, 72, 112 };
        // maxHp
        private static global::System.ReadOnlySpan<byte> GetSpan_maxHp() => new byte[1 + 5] { 165, 109, 97, 120, 72, 112 };
        // curMp
        private static global::System.ReadOnlySpan<byte> GetSpan_curMp() => new byte[1 + 5] { 165, 99, 117, 114, 77, 112 };
        // maxMp
        private static global::System.ReadOnlySpan<byte> GetSpan_maxMp() => new byte[1 + 5] { 165, 109, 97, 120, 77, 112 };
        // totalOnlineDuration
        private static global::System.ReadOnlySpan<byte> GetSpan_totalOnlineDuration() => new byte[1 + 19] { 179, 116, 111, 116, 97, 108, 79, 110, 108, 105, 110, 101, 68, 117, 114, 97, 116, 105, 111, 110 };
        // todayOnlineDuration
        private static global::System.ReadOnlySpan<byte> GetSpan_todayOnlineDuration() => new byte[1 + 19] { 179, 116, 111, 100, 97, 121, 79, 110, 108, 105, 110, 101, 68, 117, 114, 97, 116, 105, 111, 110 };
        // dialogueIds
        private static global::System.ReadOnlySpan<byte> GetSpan_dialogueIds() => new byte[1 + 11] { 171, 100, 105, 97, 108, 111, 103, 117, 101, 73, 100, 115 };

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::PlayerRoleData value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNil();
                return;
            }

            var formatterResolver = options.Resolver;
            writer.WriteMapHeader(15);
            writer.WriteRaw(GetSpan_playerBornPos());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<float>>(formatterResolver).Serialize(ref writer, value.playerBornPos, options);
            writer.WriteRaw(GetSpan_playerPos());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<float>>(formatterResolver).Serialize(ref writer, value.playerPos, options);
            writer.WriteRaw(GetSpan_playerRotate());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<float>>(formatterResolver).Serialize(ref writer, value.playerRotate, options);
            writer.WriteRaw(GetSpan_cameraRotate());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<float>>(formatterResolver).Serialize(ref writer, value.cameraRotate, options);
            writer.WriteRaw(GetSpan_firstEntryLevel());
            writer.Write(value.firstEntryLevel);
            writer.WriteRaw(GetSpan_levelId());
            writer.Write(value.levelId);
            writer.WriteRaw(GetSpan_curExp());
            writer.Write(value.curExp);
            writer.WriteRaw(GetSpan_totalExp());
            writer.Write(value.totalExp);
            writer.WriteRaw(GetSpan_curHp());
            writer.Write(value.curHp);
            writer.WriteRaw(GetSpan_maxHp());
            writer.Write(value.maxHp);
            writer.WriteRaw(GetSpan_curMp());
            writer.Write(value.curMp);
            writer.WriteRaw(GetSpan_maxMp());
            writer.Write(value.maxMp);
            writer.WriteRaw(GetSpan_totalOnlineDuration());
            writer.Write(value.totalOnlineDuration);
            writer.WriteRaw(GetSpan_todayOnlineDuration());
            writer.Write(value.todayOnlineDuration);
            writer.WriteRaw(GetSpan_dialogueIds());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<int, int>>(formatterResolver).Serialize(ref writer, value.dialogueIds, options);
        }

        public global::PlayerRoleData Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            var formatterResolver = options.Resolver;
            var length = reader.ReadMapHeader();
            var ____result = new global::PlayerRoleData();

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.Internal.CodeGenHelpers.ReadStringSpan(ref reader);
                switch (stringKey.Length)
                {
                    default:
                    FAIL:
                      reader.Skip();
                      continue;
                    case 13:
                        if (!global::System.MemoryExtensions.SequenceEqual(stringKey, GetSpan_playerBornPos().Slice(1))) { goto FAIL; }

                        ____result.playerBornPos = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<float>>(formatterResolver).Deserialize(ref reader, options);
                        continue;
                    case 9:
                        if (!global::System.MemoryExtensions.SequenceEqual(stringKey, GetSpan_playerPos().Slice(1))) { goto FAIL; }

                        ____result.playerPos = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<float>>(formatterResolver).Deserialize(ref reader, options);
                        continue;
                    case 12:
                        switch (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey))
                        {
                            default: goto FAIL;
                            case 8021599666453965936UL:
                                if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 1702125940UL) { goto FAIL; }

                                ____result.playerRotate = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<float>>(formatterResolver).Deserialize(ref reader, options);
                                continue;

                            case 8021581030256107875UL:
                                if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 1702125940UL) { goto FAIL; }

                                ____result.cameraRotate = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<float>>(formatterResolver).Deserialize(ref reader, options);
                                continue;

                        }
                    case 15:
                        if (!global::System.MemoryExtensions.SequenceEqual(stringKey, GetSpan_firstEntryLevel().Slice(1))) { goto FAIL; }

                        ____result.firstEntryLevel = reader.ReadBoolean();
                        continue;
                    case 7:
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 28228227578619244UL) { goto FAIL; }

                        ____result.levelId = reader.ReadInt32();
                        continue;
                    case 6:
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 123661863515491UL) { goto FAIL; }

                        ____result.curExp = reader.ReadInt32();
                        continue;
                    case 8:
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 8104303861247012724UL) { goto FAIL; }

                        ____result.totalExp = reader.ReadInt32();
                        continue;
                    case 5:
                        switch (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey))
                        {
                            default: goto FAIL;
                            case 482251797859UL:
                                ____result.curHp = reader.ReadInt32();
                                continue;
                            case 482252185965UL:
                                ____result.maxHp = reader.ReadInt32();
                                continue;
                            case 482335683939UL:
                                ____result.curMp = reader.ReadInt32();
                                continue;
                            case 482336072045UL:
                                ____result.maxMp = reader.ReadInt32();
                                continue;
                        }
                    case 19:
                        switch (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey))
                        {
                            default: goto FAIL;
                            case 7813269730444472180UL:
                                if (!global::System.MemoryExtensions.SequenceEqual(stringKey, GetSpan_totalOnlineDuration().Slice(1 + 8))) { goto FAIL; }

                                ____result.totalOnlineDuration = reader.ReadInt32();
                                continue;

                            case 7813269786277998452UL:
                                if (!global::System.MemoryExtensions.SequenceEqual(stringKey, GetSpan_todayOnlineDuration().Slice(1 + 8))) { goto FAIL; }

                                ____result.todayOnlineDuration = reader.ReadInt32();
                                continue;

                        }
                    case 11:
                        if (!global::System.MemoryExtensions.SequenceEqual(stringKey, GetSpan_dialogueIds().Slice(1))) { goto FAIL; }

                        ____result.dialogueIds = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<int, int>>(formatterResolver).Deserialize(ref reader, options);
                        continue;

                }
            }

            reader.Depth--;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1129 // Do not use default value type constructor
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1403 // File may only contain a single namespace
#pragma warning restore SA1649 // File name should match first type name

