using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace WpfApp1
{
  public class ViewModel : INotifyPropertyChanged
  {
    public ViewModel()
    {
      PopulateDataGrid();
      cvs.Source = dt.DefaultView;
      cvs.View.CurrentChanged += (sender, e) =>
      {
        OnPropertyChanged(nameof(SelectedServerCount));
        OnPropertyChanged(nameof(Cmd));
      };
      // add IsChecked column if not exists
      if (dt.Columns["IsChecked"] == null) dt.Columns.Add("IsChecked", typeof(bool));
      // reset IsChecked in all rows
      for (int i = 0; i < dt.Rows.Count; i++) dt.Rows[i]["IsChecked"] = false;
    }

    private DataTable dt = null;
    private CollectionViewSource cvs = new CollectionViewSource();
    public ICollectionView InventorySource { get => cvs.View; }
    public int SelectedServerCount
    {
      get
      {
        return (from row in dt.AsEnumerable() where ((DataRow)row).Field<bool>("IsChecked") select row).Count();
      }
    }
    public ICommand Cmd { get => new WpfApp1.Helper.RelayCommand(CmdExecute, CmdCanExecute); }


    public void CmdExecute(object parameter)
    {
      // execute deploy
    }
    public bool CmdCanExecute(object parameter) => SelectedServerCount > 0;

    public void PopulateDataGrid()
    {
      try
      {
        string strConnection = "Data Source=localhost\\DEV1;Initial Catalog=inventory;Integrated Security=True";
        using (SqlConnection con = new SqlConnection(strConnection))
        {
          using (SqlCommand sqlCmd = new SqlCommand())
          {
            sqlCmd.Connection = con;
            sqlCmd.CommandText = "EXEC sp_deployer_getserverlist ";
            using (SqlDataAdapter sqlDataAdap = new SqlDataAdapter(sqlCmd))
            {
              dt = new DataTable("ServerList");
              //sqlDataAdap.Fill(dt);
              //
              // for demo only
              FillDataTable(dt);
              //
            }
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }

    /// <summary>
    /// only for demo instead of SQL database
    /// </summary>
    /// <param name="dt"></param>
    private void FillDataTable(DataTable dt)
    {
      dt.Columns.Add("Server_Id", typeof(int));
      dt.Columns.Add("Server_Name", typeof(string));
      for (int i = 10; i < 30; i++) dt.Rows.Add(i, $"Server {i}");
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
  }
}
