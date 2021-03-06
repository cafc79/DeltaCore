using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using DeltaCore.Utilities;

namespace DeltaCore.DataAccess.DBConnect
{

    public class OraConexion : IDBConnect
    {
        #region "Elementos de la Clase"

        protected OracleCommand cmd = new OracleCommand();
        protected OracleConnection cnx = new OracleConnection();

        #endregion

        #region "M�todos Base"

        /// <summary>
        /// Constructor, inicializa la clase creando el objeto de conexion con la cadena de conexi�n, un command y los relaciona.
        /// Crea una nueva instancia del la clase �conexion� y se conecta por default a la base de datos �bdnsar�
        /// </summary>    
        public void DBConnect(string DB)
        {
            cnx = new OracleConnection();
            cmd = new OracleCommand {Connection = cnx};

            try
            {
                cnx.ConnectionString = DB;
            }
            catch (OracleException sqlExcep)
            {
                throw new Exception(sqlExcep.Message, sqlExcep);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        
        /// <summary>
        /// Este m�todo, verifica si la conexion con la base de datos esta disponible
        /// </summary>
        public bool Monitor()
        {
            try
            {
                if (cnx.State == System.Data.ConnectionState.Closed)
                {
                    cnx.Open();
                }
                return true;
            }
            catch (OracleException sqlExcep)
            {
                throw new Exception(sqlExcep.Message, sqlExcep);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public bool Closer()
        {
            try
            {
                if (cnx.State == System.Data.ConnectionState.Open)
                    cnx.Close();
                return true;
            }
            catch (OracleException sqlExcep)
            {
                throw new Exception(sqlExcep.Message, sqlExcep);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion

        #region Wizard

        private void LoadParameters(Dictionary<string, object> sqlParams)
        {
            if (sqlParams == null || sqlParams.Count <= 0) return;
            foreach (var param in sqlParams)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }
        }

        private DataTable Reader2Table()
        {
            var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            if (dr.HasRows)
            {
                var dt = new DataTable();
                dt.Load(dr);
                return dt;
            }
            return null;
        }

        private List<object> Query2List(object objBase)
        {
            var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            if (dr.HasRows)
            {
                var lista = new List<object>();
                var swap = SystemHelper.GetPropiedades(objBase);
                while (dr.Read())
                {
                    var registro = new List<object>();
                    for (int i = 0; i <= swap.Count - 1; i++)
                        registro.Add(dr.GetValue(dr.GetOrdinal(swap[i])));
                    lista.Add(registro);
                }
                return lista;
            }
            return null;
        }

        private DataTable StpAdapter2Table()
        {
            var da = new OracleDataAdapter();
            var dt = new DataTable();
            da.SelectCommand = cmd;
            da.Fill(dt);
            return dt;
        }

        private DataTable Adapter2Table()
        {
            var da = new OracleDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        /// <summary>
        /// Reinicia los valores iniciales del command
        /// </summary>
        private void Refresh()
        {
            cmd.CommandText = string.Empty;
            cmd.Parameters.Clear();
        }

        #endregion

        #region DB Methods

        public void ContinuesQry(string stp, Dictionary<string, object> SqlParams)
        {
            Refresh();
            try
            {
                cmd.CommandText = stp;
                LoadParameters(SqlParams);
                cmd.ExecuteNonQuery();
            }
            catch (OracleException SqlExcep)
            {
                throw new Exception(SqlExcep.Message, SqlExcep);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void ContinuesQry(string stp)
        {
            Refresh();
            try
            {
                cmd.CommandText = stp;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }
            catch (OracleException SqlExcep)
            {
                throw new Exception(SqlExcep.Message, SqlExcep);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void ExecQry(string stp, Dictionary<string, object> SqlParams)
        {
            Refresh();
            try
            {
                cmd.CommandText = stp;
                cmd.CommandType = CommandType.StoredProcedure;
                LoadParameters(SqlParams);
                Monitor();
                cmd.ExecuteNonQuery();
            }
            catch (OracleException SqlExcep)
            {
                throw new Exception(SqlExcep.Message, SqlExcep);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                Closer();
            }
        }

        public void ExecQry(string stp)
        {
            Refresh();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = stp;
                Monitor();
                cmd.ExecuteNonQuery();
            }
            catch (OracleException SqlExcep)
            {
                throw new Exception(SqlExcep.Message, SqlExcep);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                Closer();
            }
        }

        public void ExecStp(string stp, Dictionary<string, object> SqlParams, OracleTransaction Trans)
        {
            Refresh();
            try
            {
                cmd.CommandText = stp;
                cmd.CommandType = CommandType.StoredProcedure;
                LoadParameters(SqlParams);
                cmd.Transaction = Trans;
                Monitor();
                cmd.ExecuteNonQuery();
            }
            catch (OracleException SqlExcep)
            {
                throw new Exception(SqlExcep.Message, SqlExcep);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                Closer();
            }
        }

        public DataTable GetSelect(string Program)
        {
            Refresh();
            try
            {
                cmd.CommandText = Program;
                Monitor();
                return Adapter2Table();
            }
            catch (OracleException SqlExcep)
            {
                throw new Exception(SqlExcep.Message, SqlExcep);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                Closer();
            }
        }

        public DataTable ExecStp(string sql, Dictionary<string, object> SqlParams)
        {
            Refresh();
            try
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.StoredProcedure;
                LoadParameters(SqlParams);
                Monitor();
                return StpAdapter2Table();
            }
            catch (OracleException SqlExcep)
            {
                throw new Exception(SqlExcep.Message, SqlExcep);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                Closer();
            }
        }

        public List<object> ExecQry(string Query, Dictionary<string, object> SqlParams, object objBase)
        {
            Refresh();
            try
            {
                cmd.CommandText = Query;
                cmd.CommandType = CommandType.Text;
                LoadParameters(SqlParams);
                Monitor();
                return Query2List(objBase);
            }
            catch (OracleException SqlExcep)
            {
                //return null;
                throw new Exception(SqlExcep.Message, SqlExcep);
            }
            catch (Exception ex)
            {
                //return null;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (cmd.Connection.State == System.Data.ConnectionState.Open)
                {
                    cmd.Connection.Close();
                }
            }
        }

        public object ExecScalar(string sql)
        {
            Refresh();
            try
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.StoredProcedure;
                Monitor();
                return cmd.ExecuteScalar();
            }
            catch (OracleException SqlExcep)
            {
                throw new Exception(SqlExcep.Message, SqlExcep);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                Closer();
            }
        }

        public DataRow BusquedaReg(string SQL, int Index)
        {
            Refresh();
            try
            {
                cmd.CommandText = SQL;
                cmd.CommandType = CommandType.Text;
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                return Reader2Table().Rows[Index];
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                {
                    cmd.Connection.Close();
                }
            }
        }

        #endregion
    }
}