using System;
using System.Data.Common;
using Xunit;

namespace AdoNet.Specification.Tests
{
	public class DataReaderTestBase<TFixture> : DbFactoryTestBase<TFixture>
		where TFixture : class, IDbFactoryFixture
	{
		public DataReaderTestBase(TFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public virtual void Depth_returns_zero()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					Assert.Equal(0, reader.Depth);
				}
			}
		}

		[Fact]
		public virtual void FieldCount_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					Assert.Equal(1, reader.FieldCount);
				}
			}
		}

		[Fact]
		public virtual void FieldCount_throws_when_closed()
			=> X_throws_when_closed(
				r =>
				{
					var x = r.FieldCount;
				});

		[Fact]
		public virtual void GetBoolean_works()
			=> GetX_works(
				$"SELECT {Fixture.CreateBooleanLiteral(true)};",
				r => r.GetBoolean(0),
				true);

		[Fact]
		public virtual void GetByte_works()
			=> GetX_works(
				"SELECT 1;",
				r => r.GetByte(0),
				(byte) 1);

		[Fact]
		public virtual void GetBytes_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = $"SELECT {Fixture.CreateHexLiteral(new byte[] { 0x7E, 0x57 })};";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();
					Assert.True(hasData);

					var buffer = new byte[2];
					Assert.Equal(2, reader.GetBytes(0, 0, buffer, 0, buffer.Length));
					Assert.Equal(new byte[] { 0x7E, 0x57 }, buffer);
				}
			}
		}

		[Fact]
		public virtual void GetChars_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 'test';";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();
					Assert.True(hasData);

					var buffer = new char[4];
					Assert.Equal(4, reader.GetChars(0, 0, buffer, 0, buffer.Length));
					Assert.Equal(new[] { 't', 'e', 's', 't' }, buffer);
				}
			}
		}

		[Fact]
		public virtual void GetDateTime_works_with_text()
			=> GetX_works(
				"SELECT '2014-04-15 10:47:16';",
				r => r.GetDateTime(0),
				new DateTime(2014, 4, 15, 10, 47, 16));

		[Fact]
		public virtual void GetDateTime_throws_when_null()
			=> GetX_throws_when_null(r => r.GetDateTime(0));
		
		[Fact]
		public virtual void GetDataTypeName_throws_when_ordinal_out_of_range()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					Assert.Throws<IndexOutOfRangeException>(() => reader.GetDataTypeName(1));
				}
			}
		}

		[Fact]
		public virtual void GetDataTypeName_throws_when_closed()
			=> X_throws_when_closed(r => r.GetDataTypeName(0));
		
		[Fact]
		public virtual void GetDecimal_throws_when_null()
			=> GetX_throws_when_null(r => r.GetDecimal(0));

		[Fact]
		public virtual void GetDouble_throws_when_null()
			=> GetX_throws_when_null(
				r => r.GetDouble(0));

		[Fact]
		public virtual void GetEnumerator_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();
					Assert.True(hasData);

					Assert.NotNull(reader.GetEnumerator());
				}
			}
		}

		private void GetFieldValue_works<T>(string sql, T expected)
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = sql;
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();

					Assert.True(hasData);
					Assert.Equal(expected, reader.GetFieldValue<T>(0));
				}
			}
		}

		[Fact]
		public virtual void GetFieldValue_of_string_works() => GetFieldValue_works<string>("SELECT 'test';", "test");

		[Fact]
		public virtual void GetFieldValue_of_byteArray_works()
			=> GetFieldValue_works(
				$"SELECT {Fixture.CreateHexLiteral(new byte[] { 0x7E, 0x57 })};",
				new byte[] { 0x7e, 0x57 });

		[Fact]
		public virtual void GetFieldValue_of_byteArray_empty()
			=> GetFieldValue_works(
				$"SELECT {Fixture.CreateHexLiteral(new byte[0])};",
				new byte[0]);

		[Fact]
		public virtual void GetFieldValue_of_byteArray_throws_when_null()
			=> GetX_throws_when_null(
				r => r.GetFieldValue<byte[]>(0));

		[Fact]
		public virtual void GetFieldValue_of_DBNull_works()
			=> GetFieldValue_works(
				"SELECT NULL;",
				DBNull.Value);

		[Fact]
		public virtual void GetFieldValue_of_DBNull_throws_when_not_null()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();

					Assert.True(hasData);
					Assert.Throws<InvalidCastException>(() => reader.GetFieldValue<DBNull>(0));
				}
			}
		}

		[Fact]
		public virtual void GetFieldValue_throws_before_read()
			=> X_throws_before_read(r => r.GetFieldValue<DBNull>(0));

		[Fact]
		public virtual void GetFieldValue_throws_when_done()
			=> X_throws_when_done(r => r.GetFieldValue<DBNull>(0));

		[Fact]
		public virtual void GetFieldType_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 'test';";
				using (var reader = command.ExecuteReader())
				{
					Assert.Equal(typeof(string), reader.GetFieldType(0));
				}
			}
		}

		[Fact]
		public virtual void GetFieldType_throws_when_ordinal_out_of_range()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					Assert.Throws<IndexOutOfRangeException>(() => reader.GetFieldType(1));
				}
			}
		}

		[Fact]
		public virtual void GetFieldType_throws_when_closed()
			=> X_throws_when_closed(r => r.GetFieldType(0));

		[Fact]
		public virtual void GetFloat_works()
			=> GetX_works(
				"SELECT 3",
				r => r.GetFloat(0),
				3);

		[Fact]
		public virtual void GetGuid_works_when_blob()
			=> GetX_works(
				"SELECT X'0E7E0DDC5D364849AB9B8CA8056BF93A';",
				r => r.GetGuid(0),
				new Guid("dc0d7e0e-365d-4948-ab9b-8ca8056bf93a"));

		[Fact]
		public virtual void GetGuid_works_when_text()
			=> GetX_works(
				"SELECT 'dc0d7e0e-365d-4948-ab9b-8ca8056bf93a';",
				r => r.GetGuid(0),
				new Guid("dc0d7e0e-365d-4948-ab9b-8ca8056bf93a"));

		[Fact]
		public virtual void GetGuid_throws_when_null()
			=> GetX_throws_when_null(r => r.GetGuid(0));

		[Fact]
		public virtual void GetInt16_works()
			=> GetX_works(
				"SELECT 1;",
				r => r.GetInt16(0),
				(short) 1);

		[Fact]
		public virtual void GetInt32_works()
			=> GetX_works(
				"SELECT 1;",
				r => r.GetInt32(0),
				1);

		[Fact]
		public virtual void GetInt64_works()
			=> GetX_works(
				"SELECT 1;",
				r => r.GetInt64(0),
				1L);

		[Fact]
		public virtual void GetInt64_throws_when_null()
			=> GetX_throws_when_null(
				r => r.GetInt64(0));

		[Fact]
		public virtual void GetName_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1 AS Id;";
				using (var reader = command.ExecuteReader())
				{
					Assert.Equal("Id", reader.GetName(0));
				}
			}
		}

		[Fact]
		public virtual void GetName_throws_when_ordinal_out_of_range()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					Assert.Throws<IndexOutOfRangeException>(() => reader.GetName(1));
				}
			}
		}

		[Fact]
		public virtual void GetName_throws_when_closed()
			=> X_throws_when_closed(r => r.GetName(0));

		[Fact]
		public virtual void GetOrdinal_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1 AS Id;";
				using (var reader = command.ExecuteReader())
				{
					Assert.Equal(0, reader.GetOrdinal("Id"));
				}
			}
		}

		[Fact]
		public virtual void GetOrdinal_throws_when_out_of_range()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					Assert.Throws<IndexOutOfRangeException>(() => reader.GetOrdinal("Name"));
				}
			}
		}

		[Fact]
		public virtual void GetString_works_utf8_two_bytes() => GetX_works("SELECT 'Ä';", r => r.GetString(0), "Ä");

		[Fact]
		public virtual void GetString_works_utf8_three_bytes() => GetX_works("SELECT 'Ḁ';", r => r.GetString(0), "Ḁ");

		[Fact]
		public virtual void GetString_works_utf8_four_bytes() => GetX_works("SELECT '😀';", r => r.GetString(0), "😀");

		[Fact]
		public virtual void GetFieldValue_works_utf8_two_bytes() => GetX_works("SELECT 'Ä';", r => r.GetFieldValue<string>(0), "Ä");

		[Fact]
		public virtual void GetFieldValue_works_utf8_three_bytes() => GetX_works("SELECT 'Ḁ';", r => r.GetFieldValue<string>(0), "Ḁ");

		[Fact]
		public virtual void GetFieldValue_works_utf8_four_bytes() => GetX_works("SELECT '😀';", r => r.GetFieldValue<string>(0), "😀");

		[Fact]
		public virtual void GetValue_to_string_works_utf8_two_bytes() => GetX_works("SELECT 'Ä';", r => r.GetValue(0) as string, "Ä");

		[Fact]
		public virtual void GetValue_to_string_works_utf8_three_bytes() => GetX_works("SELECT 'Ḁ';", r => r.GetValue(0) as string, "Ḁ");

		[Fact]
		public virtual void GetValue_to_string_works_utf8_four_bytes() => GetX_works("SELECT '😀';", r => r.GetValue(0) as string, "😀");

		[Fact]
		public virtual void GetString_works()
			=> GetX_works(
				"SELECT 'test';",
				r => r.GetString(0),
				"test");

		[Fact]
		public virtual void GetString_throws_when_null()
			=> GetX_throws_when_null(
				r => r.GetString(0));

		private void GetValue_works(string sql, object expected)
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = sql;
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();

					Assert.True(hasData);
					Assert.Equal(expected, reader.GetValue(0));
				}
			}
		}

		[Fact]
		public virtual void GetValue_works_when_string() => GetValue_works("SELECT 'test';", "test");

		[Fact]
		public virtual void GetValue_works_when_blob()
			=> GetValue_works(
				$"SELECT {Fixture.CreateHexLiteral(new byte[] { 0x7E, 0x57 })};",
				new byte[] { 0x7e, 0x57 });

		[Fact]
		public virtual void GetValue_works_when_null()
			=> GetValue_works(
				"SELECT NULL;",
				DBNull.Value);

		[Fact]
		public virtual void GetValue_throws_before_read()
			=> X_throws_before_read(r => r.GetValue(0));

		[Fact]
		public virtual void GetValue_throws_when_done()
			=> X_throws_when_done(r => r.GetValue(0));

		[Fact]
		public virtual void GetValue_throws_when_closed()
			=> X_throws_when_closed(r => r.GetValue(0));

		[Fact]
		public virtual void GetValues_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 'a', 'b';";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();
					Assert.True(hasData);

					// Array may be wider than row
					var values = new object[3];
					var result = reader.GetValues(values);

					Assert.Equal(2, result);
					Assert.Equal("a", values[0]);
					Assert.Equal("b", values[1]);
				}
			}
		}

		[Fact]
		public virtual void GetValues_when_too_narrow()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();
					Assert.True(hasData);

					var values = new object[0];
					Assert.Equal(0, reader.GetValues(values));
				}
			}
		}

		[Fact]
		public virtual void HasRows_returns_true_when_rows()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					Assert.True(reader.HasRows);
				}
			}
		}

		[Fact]
		public virtual void HasRows_returns_false_when_no_rows()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = Fixture.SelectNoRows;
				using (var reader = command.ExecuteReader())
				{
					Assert.False(reader.HasRows);
				}
			}
		}

		[Fact]
		public virtual void HasRows_works_when_batching()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = Fixture.SelectNoRows + "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					Assert.False(reader.HasRows);

					reader.NextResult();

					Assert.True(reader.HasRows);
				}
			}
		}

		[Fact]
		public virtual void IsClosed_returns_false_when_active()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					Assert.False(reader.IsClosed);
				}
			}
		}

		[Fact]
		public virtual void IsClosed_returns_true_when_closed()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				var reader = command.ExecuteReader();
				reader.Close();

				Assert.True(reader.IsClosed);
			}
		}

		[Fact]
		public virtual void IsDBNull_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT NULL;";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();

					Assert.True(hasData);
					Assert.True(reader.IsDBNull(0));
				}
			}
		}

		[Fact]
		public virtual void IsDBNull_throws_before_read()
			=> X_throws_before_read(r => r.IsDBNull(0));

		[Fact]
		public virtual void IsDBNull_throws_when_done()
			=> X_throws_when_done(r => r.IsDBNull(0));

		[Fact]
		public virtual void IsDBNull_throws_when_closed()
			=> X_throws_when_closed(r => r.IsDBNull(0));

		[Fact]
		public virtual void Item_by_ordinal_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 'test';";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();
					Assert.True(hasData);

					Assert.Equal("test", reader[0]);
				}
			}
		}

		[Fact]
		public virtual void Item_by_name_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 'test' AS Id;";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();
					Assert.True(hasData);

					Assert.Equal("test", reader["Id"]);
				}
			}
		}

		[Fact]
		public virtual void NextResult_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1; SELECT 2";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();
					Assert.True(hasData);
					Assert.Equal(1L, reader.GetInt64(0));

					var hasResults = reader.NextResult();
					Assert.True(hasResults);

					hasData = reader.Read();
					Assert.True(hasData);
					Assert.Equal(2L, reader.GetInt64(0));

					hasResults = reader.NextResult();
					Assert.False(hasResults);
				}
			}
		}

		[Fact]
		public virtual void NextResult_can_be_called_more_than_once()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				using (var reader = command.ExecuteReader())
				{
					var hasResults = reader.NextResult();
					Assert.False(hasResults);

					hasResults = reader.NextResult();
					Assert.False(hasResults);
				}
			}
		}
		
		[Fact]
		public virtual void Read_works()
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1 UNION SELECT 2;";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();
					Assert.True(hasData);
					Assert.Equal(1L, reader.GetInt64(0));

					hasData = reader.Read();
					Assert.True(hasData);
					Assert.Equal(2L, reader.GetInt64(0));

					hasData = reader.Read();
					Assert.False(hasData);
				}
			}
		}

		[Fact]
		public virtual void Read_throws_when_closed() => X_throws_when_closed(r => r.Read());

		[Fact]
		public virtual void NextResult_throws_when_closed() => X_throws_when_closed(r => r.NextResult());

		private void GetX_works<T>(string sql, Func<DbDataReader, T> action, T expected)
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = sql;
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();

					Assert.True(hasData);
					Assert.Equal(expected, action(reader));
				}
			}
		}

		private void GetX_throws_when_null(Action<DbDataReader> action)
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT NULL;";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();

					Assert.True(hasData);
					Assert.Throws<InvalidCastException>(() => action(reader));
				}
			}
		}

		private void X_throws_before_read(Action<DbDataReader> action)
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT NULL;";
				using (var reader = command.ExecuteReader())
				{
					Assert.Throws<InvalidOperationException>(() => action(reader));
				}
			}
		}

		private void X_throws_when_done(Action<DbDataReader> action)
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT NULL;";
				using (var reader = command.ExecuteReader())
				{
					var hasData = reader.Read();
					Assert.True(hasData);

					hasData = reader.Read();
					Assert.False(hasData);

					Assert.Throws<InvalidOperationException>(() => action(reader));
				}
			}
		}

		private void X_throws_when_closed(Action<DbDataReader> action)
		{
			using (var connection = CreateOpenConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT 1;";
				var reader = command.ExecuteReader();
				((IDisposable) reader).Dispose();

				Assert.Throws<InvalidOperationException>(() => action(reader));
			}
		}

		private enum MyEnum
		{
			One = 1
		}
	}
}
