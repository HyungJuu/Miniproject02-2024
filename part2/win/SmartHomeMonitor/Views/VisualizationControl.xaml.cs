using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SmartHomeMonitoringApp.Logics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartHomeMonitoringApp.Views
{
    /// <summary>
    /// VisualizationControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VisualizationControl : UserControl
    {
        public VisualizationControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DtpStart.Text = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
            DtpEnd.Text = DateTime.Now.ToString("yyyy-MM-dd");
        }

        // 검색버튼 클릭 이벤트 핸들러
        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataSet ds = new DataSet(); // 실제 데이터를 담을 객체

            using (SqlConnection conn = new SqlConnection(Commons.DBCONNSTRING))
            {
                conn.Open();

                var selQuery = @"SELECT [Idx]
                                      , [DEV_ID]
                                      , [CURR_DT]
                                      , [TEMP]
                                      , [HUMID]
                                   FROM [dbo].[SmartHomeData]
                                  WHERE CONVERT(CHAR(10),CURR_DT, 23) BETWEEN @StartDt AND @EndDt";

                SqlCommand cmd = new SqlCommand(selQuery, conn);
                cmd.Parameters.AddWithValue("@StartDt", DtpStart.Text);
                cmd.Parameters.AddWithValue(@"EndDt", DtpEnd.Text);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds, "SmartHomeData");
            }

            //MessageBox.Show("TotalData", ds.Tables["SmartHomeData"].Rows.Count.ToString());
            LblTotalCount.Content = $"검색데이터 : {ds.Tables["SmartHomeData"].Rows.Count} 개";

            var title = new LabelVisual
            {
                Text = "SmartHome Visualization",
                TextSize = 16,
                Padding = new LiveChartsCore.Drawing.Padding(15),
            };
            ChtResult.Title = title;

            var series = new LineSeries<double> { Fill = null };
            foreach (DataRow item in ds.Tables["SmartHomeData"].Rows)
            {
                series.Values.Append(Convert.ToDouble(item[4]));
            }
            ChtResult.Series = series;
        }
    }
}
