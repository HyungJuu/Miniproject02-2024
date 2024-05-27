using MahApps.Metro.Controls;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using SmartHomeMonitoringApp.Logics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// DataBaseControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DataBaseControl : UserControl
    {
        // 변수, 속성 초기화 선언
        public Boolean IsConnected { get; set; }
        Thread MqttThread { get; set; } // 없으면 UI컨트롤과 충돌나서 Log를 못찍음(응답없음)
        int MaxCount { get; set; } = 10; // MQTT 로그 과적으로 인한 속도저하를 방지하기 위해서


        public DataBaseControl()
        {
            InitializeComponent();
        }

        // 유저 컨트롤 화면 로드 후 초기화
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TxtBrokerUrl.Text = Commons.BROKERHOST;
            TxtMqttTopic.Text = Commons.MQTTTOPIC;
            TxtConnString.Text = Commons.DBCONNSTRING;

            IsConnected = false;
            BtnConnect.IsChecked = false;

        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectSystem(); // DB, MQTT 연결
        }

        private async void ConnectSystem()
        {
            if (IsConnected == false) // 연결이 안됐으면 처리,
            {
                // 한번도 접속을 안했으면 모두 연결
                var mqttFactory = new MqttFactory();
                Commons.MQTT_CLIENT = mqttFactory.CreateMqttClient();
                // MQTT 브로커 아이피 연결속성
                var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(TxtBrokerUrl.Text).Build();

                await Commons.MQTT_CLIENT.ConnectAsync(mqttClientOptions, CancellationToken.None);
                Commons.MQTT_CLIENT.ApplicationMessageReceivedAsync += MQTT_CLIENT_ApplicationMessageReceivedAsync;

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder().WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(Commons.MQTTTOPIC);
                    }).Build();

                await Commons.MQTT_CLIENT.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

                IsConnected = true;
                BtnConnect.IsChecked = true;
                BtnConnect.Content = "MQTT 연결중";
            }
            else
            {
                // 연결 후 연결끊기
                if (Commons.MQTT_CLIENT.IsConnected)
                {
                    Commons.MQTT_CLIENT.ApplicationMessageReceivedAsync -= MQTT_CLIENT_ApplicationMessageReceivedAsync;
                    await Commons.MQTT_CLIENT.DisconnectAsync();

                    IsConnected = false;
                    BtnConnect.IsChecked = false;
                    BtnConnect.Content = "Connect";
                }
            }
        }

        private Task MQTT_CLIENT_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
            //Debug.WriteLine("MQTT 기본연결!");
            Debug.WriteLine(payload);
            UpdateLog(payload); //  스레드처리

            return Task.CompletedTask; // Async에서 Task값을 넘기려면 이렇게
        }

        private void UpdateLog(string payload)
        {
            this.Invoke(() =>
            {
                TxtLog.Text += $"{payload}\n";
                TxtLog.ScrollToEnd(); // 스크롤이 생기기 시작하면 제일 아래로 포커스
            });
        }
    }
}
