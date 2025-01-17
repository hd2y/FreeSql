﻿using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Xunit;

namespace FreeSql.Tests.Sqlite
{
    public class SqliteCodeFirstTest
    {
        [Fact]
        public void Test_0String()
        {
            var fsql = g.sqlite;
            fsql.Delete<test_0string01>().Where("1=1").ExecuteAffrows();

            Assert.Equal(1, fsql.Insert(new test_0string01 { name = @"1.0000\0.0000\0.0000\0.0000\1.0000\0.0000" }).ExecuteAffrows());
            Assert.Equal(1, fsql.Insert(new test_0string01 { name = @"1.0000\0.0000\0.0000\0.0000\1.0000\0.0000" }).NoneParameter().ExecuteAffrows());

            var list = fsql.Select<test_0string01>().ToList();
            Assert.Equal(2, list.Count);
            Assert.Equal(@"1.0000\0.0000\0.0000\0.0000\1.0000\0.0000", list[0].name);
            Assert.Equal(@"1.0000\0.0000\0.0000\0.0000\1.0000\0.0000", list[1].name);
        }
        class test_0string01
        {
            public Guid id { get; set; }
            public string name { get; set; }
        }

        [Fact]
        public void InsertUpdateParameter()
        {
            var fsql = g.sqlite;
            fsql.CodeFirst.SyncStructure<ts_iupstr_bak>();
            var item = new ts_iupstr { id = Guid.NewGuid(), title = string.Join(",", Enumerable.Range(0, 2000).Select(a => "我是中国人")) };
            Assert.Equal(1, fsql.Insert(item).ExecuteAffrows());
            var find = fsql.Select<ts_iupstr>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(find.id, item.id);
            Assert.Equal(find.title, item.title);
        }
        [Table(Name = "ts_iupstr_bak", DisableSyncStructure = true)]
        class ts_iupstr
        {
            public Guid id { get; set; }
            public string title { get; set; }
        }
        class ts_iupstr_bak
        {
            public Guid id { get; set; }
            [Column(StringLength = -1)]
            public string title { get; set; }
        }

        [Fact]
        public void Blob()
        {
            var str1 = string.Join(",", Enumerable.Range(0, 10000).Select(a => "我是中国人"));
            var data1 = Encoding.UTF8.GetBytes(str1);

            var item1 = new TS_BLB01 { Data = data1 };
            Assert.Equal(1, g.sqlite.Insert(item1).ExecuteAffrows());

            var item2 = g.sqlite.Select<TS_BLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(item1.Data.Length, item2.Data.Length);

            var str2 = Encoding.UTF8.GetString(item2.Data);
            Assert.Equal(str1, str2);

            //NoneParameter
            item1 = new TS_BLB01 { Data = data1 };
            Assert.Equal(1, g.sqlite.Insert<TS_BLB01>().NoneParameter().AppendData(item1).ExecuteAffrows());

            item2 = g.sqlite.Select<TS_BLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(item1.Data.Length, item2.Data.Length);

            str2 = Encoding.UTF8.GetString(item2.Data);
            Assert.Equal(str1, str2);

            Assert.Equal(1, g.sqlite.InsertOrUpdate<TS_BLB01>().SetSource(new TS_BLB01 { Data = data1 }).ExecuteAffrows());
            item2 = g.sqlite.Select<TS_BLB01>().Where(a => a.Id == item1.Id).First();
            Assert.Equal(item1.Data.Length, item2.Data.Length);

            str2 = Encoding.UTF8.GetString(item2.Data);
            Assert.Equal(str1, str2);
        }
        class TS_BLB01
        {
            public Guid Id { get; set; }
            [MaxLength(-1)]
            public byte[] Data { get; set; }
        }

        [Fact]
        public void StringLength()
        {
            var dll = g.sqlite.CodeFirst.GetComparisonDDLStatements<TS_SLTB>();
            g.sqlite.CodeFirst.SyncStructure<TS_SLTB>();
        }
        class TS_SLTB
        {
            public Guid Id { get; set; }
            [Column(StringLength = 50)]
            public string Title { get; set; }

            [Column(IsNullable = false, StringLength = 50)]
            public string TitleSub { get; set; }
        }

        [Fact]
        public void 表名中有点()
        {
            var item = new tbdot01 { name = "insert" };
            g.sqlite.Insert(item).ExecuteAffrows();

            var find = g.sqlite.Select<tbdot01>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal("insert", find.name);

            Assert.Equal(1, g.sqlite.Update<tbdot01>().Set(a => a.name == "update").Where(a => a.id == item.id).ExecuteAffrows());
            find = g.sqlite.Select<tbdot01>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal("update", find.name);

            Assert.Equal(1, g.sqlite.Delete<tbdot01>().Where(a => a.id == item.id).ExecuteAffrows());
            find = g.sqlite.Select<tbdot01>().Where(a => a.id == item.id).First();
            Assert.Null(find);
        }
        [Table(Name = "\"sys.tbdot01\"")]
        class tbdot01
        {
            public Guid id { get; set; }
            public string name { get; set; }
        }

        [Fact]
        public void 中文表_字段()
        {
            var sql = g.sqlite.CodeFirst.GetComparisonDDLStatements<测试中文表>();
            g.sqlite.CodeFirst.SyncStructure<测试中文表>();

            var item = new 测试中文表
            {
                标题 = "测试标题",
                创建时间 = DateTime.Now
            };
            Assert.Equal(1, g.sqlite.Insert<测试中文表>().AppendData(item).ExecuteAffrows());
            Assert.NotEqual(Guid.Empty, item.编号);
            var item2 = g.sqlite.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新";
            Assert.Equal(1, g.sqlite.Update<测试中文表>().SetSource(item).ExecuteAffrows());
            item2 = g.sqlite.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo";
            var repo = g.sqlite.GetRepository<测试中文表>();
            Assert.Equal(1, repo.Update(item));
            item2 = g.sqlite.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo22";
            Assert.Equal(1, repo.Update(item));
            item2 = g.sqlite.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);
        }
        class 测试中文表
        {
            [Column(IsPrimary = true)]
            public Guid 编号 { get; set; }

            public string 标题 { get; set; }

            [Column(ServerTime = DateTimeKind.Local, CanUpdate = false)]
            public DateTime 创建时间 { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime 更新时间 { get; set; }
        }

        [Fact]
        public void AddUniques()
        {
            var sql = g.sqlite.CodeFirst.GetComparisonDDLStatements<AddUniquesInfo>();
            g.sqlite.CodeFirst.SyncStructure<AddUniquesInfo>();
            g.sqlite.CodeFirst.SyncStructure(typeof(AddUniquesInfo), "AddUniquesInfo1");
        }
        [Table(Name = "AddUniquesInfo2", OldName = "AddUniquesInfo")]
        [Index("{tablename}_uk_phone", "phone", true)]
        [Index("{tablename}_uk_group_index", "group,index", true)]
        [Index("{tablename}_uk_group_index22", "group desc, index22", true)]
        class AddUniquesInfo
        {
            public Guid id { get; set; }
            public string phone { get; set; }

            public string group { get; set; }
            public int index { get; set; }
            public string index22 { get; set; }
        }

        public class Topic
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public DateTime CreateTime { get; set; }
        }
        [Table(Name = "xxxtb.Comment")]
        public class Comment
        {
            public Guid Id { get; set; }
            public Guid TopicId { get; set; }
            public virtual Topic Topic { get; set; }
            public string Nickname { get; set; }
            public string Content { get; set; }
            public DateTime CreateTime { get; set; }
        }


        [Fact]
        public void AddField()
        {

            //秀一波 FreeSql.Repository 扩展包，dotnet add package FreeSql.Repository
            var topicRepository = g.sqlite.GetGuidRepository<Topic>();
            var commentRepository = g.sqlite.GetGuidRepository<Comment>();

            //添加测试文章
            var topic = topicRepository.Insert(new Topic
            {
                Title = "文章标题1",
                Content = "文章内容1",
                CreateTime = DateTime.Now
            });

            //添加10条测试评论
            var comments = Enumerable.Range(0, 10).Select(a => new Comment
            {
                TopicId = topic.Id,
                Nickname = $"昵称{a}",
                Content = $"评论内容{a}",
                CreateTime = DateTime.Now
            }).ToArray();
            var affrows = commentRepository.Insert(comments);

            var find = commentRepository.Select.Where(a => a.Topic.Title == "文章标题1").ToList();




            var sql = g.sqlite.CodeFirst.GetComparisonDDLStatements<TopicAddField>();

            var id = g.sqlite.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteIdentity();

            //var inserted = g.Sqlite.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteInserted();
        }

        [Table(Name = "xxxtb.TopicAddField", OldName = "TopicAddField")]
        public class TopicAddField
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }

            public string name { get; set; }

            [Column(DbType = "varchar(200) not null", OldName = "title2")]
            public string title3223 { get; set; } = "10";

            [Column(IsIgnore = true)]
            public DateTime ct { get; set; } = DateTime.Now;
        }

        [Fact]
        public void GetComparisonDDLStatements()
        {

            var sql = g.sqlite.CodeFirst.GetComparisonDDLStatements<TableAllType>();
            Assert.True(string.IsNullOrEmpty(sql)); //测试运行两次后
            //sql = g.Sqlite.CodeFirst.GetComparisonDDLStatements<Tb_alltype>();
        }

        IInsert<TableAllType> insert => g.sqlite.Insert<TableAllType>();
        ISelect<TableAllType> select => g.sqlite.Select<TableAllType>();

        [Fact]
        public void CurdAllField()
        {
            var item = new TableAllType { };
            item.Id = (int)insert.AppendData(item).ExecuteIdentity();

            var newitem = select.Where(a => a.Id == item.Id).ToOne();

            var item2 = new TableAllType
            {
                Bool = true,
                BoolNullable = true,
                Byte = 255,
                ByteNullable = 127,
                Bytes = Encoding.UTF8.GetBytes("我是中国人"),
                DateTime = DateTime.Now,
                DateTimeNullable = DateTime.Now.AddHours(-1),
                Decimal = 99.99M,
                DecimalNullable = 99.98M,
                Double = 999.99,
                DoubleNullable = 999.98,
                Enum1 = TableAllTypeEnumType1.e5,
                Enum1Nullable = TableAllTypeEnumType1.e3,
                Enum2 = TableAllTypeEnumType2.f2,
                Enum2Nullable = TableAllTypeEnumType2.f3,
                Float = 19.99F,
                FloatNullable = 19.98F,
                Guid = Guid.NewGuid(),
                GuidNullable = Guid.NewGuid(),
                Int = int.MaxValue,
                IntNullable = int.MinValue,
                SByte = 100,
                SByteNullable = 99,
                Short = short.MaxValue,
                ShortNullable = short.MinValue,
                String = "我是中国人string'\\?!@#$%^&*()_+{}}{~?><<>",
                Char = 'X',
                TimeSpan = TimeSpan.FromSeconds(999),
                TimeSpanNullable = TimeSpan.FromSeconds(60),
                UInt = uint.MaxValue,
                UIntNullable = uint.MinValue,
                ULong = ulong.MaxValue - 10000000,
                ULongNullable = ulong.MinValue,
                UShort = ushort.MaxValue,
                UShortNullable = ushort.MinValue,
                testFielLongNullable = long.MinValue
            };
            item2.Id = (int)insert.AppendData(item2).ExecuteIdentity();
            var newitem2 = select.Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item2.String, newitem2.String);
            Assert.Equal(item2.Char, newitem2.Char);

            item2.Id = (int)insert.NoneParameter().AppendData(item2).ExecuteIdentity();
            newitem2 = select.Where(a => a.Id == item2.Id).ToOne();
            Assert.Equal(item2.String, newitem2.String);
            Assert.Equal(item2.Char, newitem2.Char);

            var items = select.ToList();
            var itemstb = select.ToDataTable();
        }

        [Fact]
        public void UpdateSetFlag()
        {
            var sql1 = g.sqlite.Update<TableAllType>()
                .Set(a => a.Enum2 | TableAllTypeEnumType2.f2)
                .Where(a => a.Id == 10)
                .ToSql();
            Assert.Equal(@"UPDATE ""tb_alltype"" SET ""Enum2"" = (""Enum2"" | 1), ""DateTime"" = datetime(current_timestamp,'localtime'), ""DateTimeOffSet"" = datetime(current_timestamp,'localtime'), ""DateTimeNullable"" = datetime(current_timestamp,'localtime'), ""DateTimeOffSetNullable"" = datetime(current_timestamp,'localtime') 
WHERE (""Id"" = 10)", sql1);
        }

        [Table(Name = "tb_alltype")]
        class TableAllType
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            //public string id2 { get; set; } = "id2=10";

            public bool Bool { get; set; }
            public sbyte SByte { get; set; }
            public short Short { get; set; }
            public int Int { get; set; }
            public long Long { get; set; }
            public byte Byte { get; set; }
            public ushort UShort { get; set; }
            public uint UInt { get; set; }
            public ulong ULong { get; set; }
            public double Double { get; set; }
            public float Float { get; set; }
            public decimal Decimal { get; set; }
            public TimeSpan TimeSpan { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime DateTime { get; set; }
            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime DateTimeOffSet { get; set; }

            public byte[] Bytes { get; set; }
            public string String { get; set; }
            public char Char { get; set; }
            public Guid Guid { get; set; }

            public bool? BoolNullable { get; set; }
            public sbyte? SByteNullable { get; set; }
            public short? ShortNullable { get; set; }
            public int? IntNullable { get; set; }
            public long? testFielLongNullable { get; set; }
            public byte? ByteNullable { get; set; }
            public ushort? UShortNullable { get; set; }
            public uint? UIntNullable { get; set; }
            public ulong? ULongNullable { get; set; }
            public double? DoubleNullable { get; set; }
            public float? FloatNullable { get; set; }
            public decimal? DecimalNullable { get; set; }
            public TimeSpan? TimeSpanNullable { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime? DateTimeNullable { get; set; }
            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime? DateTimeOffSetNullable { get; set; }

            public Guid? GuidNullable { get; set; }

            public TableAllTypeEnumType1 Enum1 { get; set; }
            public TableAllTypeEnumType1? Enum1Nullable { get; set; }
            public TableAllTypeEnumType2 Enum2 { get; set; }
            public TableAllTypeEnumType2? Enum2Nullable { get; set; }

            public TableAllTypeEnumType3 testFieldEnum3 { get; set; }
            public TableAllTypeEnumType3? testFieldEnum3Nullable { get; set; }

            public enum TableAllTypeEnumType3 { }
        }

        public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
        [Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
    }
}
