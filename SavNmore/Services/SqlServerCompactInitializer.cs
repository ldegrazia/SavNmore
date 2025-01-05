//================================================================================
// Reboot, Inc. Entity Framework Membership for .NET
//================================================================================
// NO unauthorized distribution of any copy of this code (including any related 
// documentation) is allowed.
// 
// The Reboot. Inc. name, trademarks and/or logo(s) of Reboot, Inc. shall not be used to 
// name (even as a part of another name), endorse and/or promote products derived 
// from this code without prior written permission from Reboot, Inc.
// 
// The use, copy, and/or distribution of this code is subject to the terms of the 
// Reboot, Inc. License Agreement. This code shall not be used, copied, 
// and/or distributed under any other license agreement.
// 
//                                         
// THIS CODE IS PROVIDED BY REBOOT, INC. 
// (“Reboot”) “AS IS” WITHOUT ANY WARRANTY OF ANY KIND. REBBOT, INC. HEREBY DISCLAIMS 
// ALL EXPRESS, IMPLIED, OR STATUTORY CONDITIONS, REPRESENTATIONS AND WARRANTIES 
// WITH RESPECT TO THIS CODE (OR ANY PART THEREOF), INCLUDING, BUT NOT LIMITED TO, 
// IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE OR 
// NON-INFRINGEMENT. REBOOT, INC. AND ITS SUPPLIERS SHALL NOT BE LIABLE FOR ANY DAMAGE 
// SUFFERED AS A RESULT OF USING THIS CODE. IN NO EVENT SHALL REBOOT, INC AND ITS 
// SUPPLIERS BE LIABLE FOR ANY DIRECT, INDIRECT, CONSEQUENTIAL, ECONOMIC, 
// INCIDENTAL, OR SPECIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, ANY LOST 
// REVENUES OR PROFITS).
// 
//                                         
// Copyright © 2012 Reboot, Inc. All rights reserved.
//================================================================================
using System;
using System.Data.SqlServerCe;
using System.Configuration;
using System.Data;
using savnmore;

namespace savnmore.Services
{
    /// <summary>
    /// Initializes the Sql Server Compact Database
    /// To rebuild entirely on model changes with Sql CE, please drop any existing tables.
    /// If you are not using Sql Server Compact, this class can be safely removed.
    /// </summary>
    public static class SqlServerCompactInitializer
    {
        /// <summary>
        /// Drops all the tables if they exist
        /// </summary>
        public static void DropAllTables()
        {
            using (
                var cn =
                    new SqlCeConnection(
                        ConfigurationManager.ConnectionStrings[Constants.ConnectionStringKey].ConnectionString))
            {
                if (cn.State == ConnectionState.Closed)
                {
                    cn.Open();
                }
                DataTable dt = cn.GetSchema("Tables");
                //const string sql = @"select 'drop table ' || table_name || ';'   from information_schema.tables;";
                foreach (DataRow dr in dt.Rows)
                {
                    var droptble = "drop table " + dr[2].ToString() + ";";
                    var cmd = new SqlCeCommand(droptble, cn);
                    cmd.ExecuteNonQuery();
                }
                cn.Close();
            }
        }
        /// <summary>
        /// Checks for the EdmMetadata table, adds the table and a record if needed, so the new model can be synced
        /// </summary>
        public static void SetupEdmMetadataTable()
        {
            using (var cn = new SqlCeConnection(ConfigurationManager.ConnectionStrings[Constants.ConnectionStringKey].ConnectionString))
            {
                if (cn.State == ConnectionState.Closed)
                {
                    cn.Open();
                }
                const string sql = @"select count(*) from EdmMetadata";
                const string sqlinsert = @"INSERT INTO EdmMetadata (ModelHash) VALUES ('foo')";
                var cmd = new SqlCeCommand(sql, cn);
                try
                {
                    object value = cmd.ExecuteScalar();
                    int recordCount = Convert.ToInt32(value);
                    switch (recordCount)
                    {
                        case 0:
                            {
                                //there are no records, but the table exists

                                cmd = new SqlCeCommand(sqlinsert, cn);
                                cmd.ExecuteNonQuery();
                                //Logger.WriteLine(MessageType.Information, "EdmMetadata Table populated with a record ...");
                                return;
                            }
                        default:
                            {
                                //there already are records, looks like we are good.
                                //Logger.WriteLine(MessageType.Information, "EdmMetadata Table populated with a record ...");
                                return;
                            }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The specified table does not exist"))
                    {
                        Logger.WriteLine(MessageType.Information, "Table Creation exception:" + ex.Message);
                        Logger.WriteLine(MessageType.Information, "EdmMetadata Table not created...");

                    }
                }
                finally
                {
                    cn.Close();
                }
                //we need to create the table
                const string createTable = @"create table EdmMetadata ([Id] [int] IDENTITY(1,1) NOT NULL, [ModelHash] [nvarchar](255) NULL)";
                if (cn.State == ConnectionState.Closed)
                {
                    cn.Open();
                }
                cmd = new SqlCeCommand(createTable, cn);
                try
                {
                    cmd.ExecuteNonQuery();
                    Logger.WriteLine(MessageType.Information, "EdmMetadata Table Created...");
                    cmd = new SqlCeCommand(sqlinsert, cn);
                    Logger.WriteLine(MessageType.Information, "ModelHash Seeded...");
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(MessageType.Information, "Table Creation exception:" + ex.Message);
                    throw;
                }
                finally
                {
                    cn.Close();
                }
            }


        }
    }
}