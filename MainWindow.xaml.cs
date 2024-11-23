using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.IO;

namespace PhotoEdit
{
    public struct Option
    {
        public int Exposure { get; set; }
        public int Brightness { get; set; }

        public int Highlight { get; set; }     
        
        public int Shadow { get; set; }

    }

    public partial class MainWindow : System.Windows.Window
    {
        private Mat originalImage; // 원본 이미지
        private Mat currentImage; // 현재 이미지 (OpenCV Mat)
        private string currentFilePath; // 현재 파일 경로

        private Option currentOptions = new Option();
        private string currentEffect = "Exposure";

        // 현재 활성화된 버튼을 추적하는 변수
        private Button currentActiveButton = null;

        public MainWindow()
        {
            InitializeComponent();
        }


        // 이미지 불러오기
        private void OpenImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    currentFilePath = openFileDialog.FileName;
                    originalImage = Cv2.ImRead(currentFilePath); // OpenCV Mat로 이미지 읽기
                    currentImage = originalImage.Clone();
                    ImageDisplay1.Source = MatToBitmapImage(currentImage); // WPF에 표시
                    // 구조체 초기화 함수 구현해야함
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"이미지를 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 이미지 저장하기
        private void SaveImage(object sender, RoutedEventArgs e)
        {
            if (currentImage == null || string.IsNullOrEmpty(currentFilePath))
            {
                MessageBox.Show("저장할 이미지가 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Cv2.ImWrite(currentFilePath, currentImage); // OpenCV Mat 저장
                MessageBox.Show("이미지가 성공적으로 저장되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"이미지를 저장하는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 다른 이름으로 저장하기
        private void SaveAsImage(object sender, RoutedEventArgs e)
        {
            if (currentImage == null)
            {
                MessageBox.Show("저장할 이미지가 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    Cv2.ImWrite(saveFileDialog.FileName, currentImage); // OpenCV Mat 저장
                    MessageBox.Show("이미지가 성공적으로 저장되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"이미지를 저장하는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // OpenCV Mat → BitmapImage 변환
        private BitmapImage MatToBitmapImage(Mat mat)
        {
            using (var bitmap = mat.ToBitmap())
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        // 이미지 크기 조절 (마우스 위치 중심 확대)
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
        }

        // 초기 화면 이미지 불러오기 버튼
        private void LoadImgButton_Click(object sender, RoutedEventArgs e)
        {
            OpenImage(sender, e);

            if (ImageDisplay1.Source != null)
            {
                LoadImgButton.Visibility = Visibility.Collapsed;
            }
        }


        //추가 효과 처리 함수들

        // 버튼 클릭
        private void ExposureButton_Click(object sender, RoutedEventArgs e)
        {
            currentEffect = "Exposure";
            UpdateSliderForCurrentEffect();
        }

        private void BrightnessButton_Click(object sender, RoutedEventArgs e)
        {
            currentEffect = "Brightness";
            UpdateSliderForCurrentEffect();
        }

        private void HighlightButton_Click(object sender, RoutedEventArgs e)
        {
            currentEffect = "Highlight";
            UpdateSliderForCurrentEffect();
        }

        private void ShadowButton_Click(object sender, RoutedEventArgs e)
        {
            currentEffect = "Shadow";
            UpdateSliderForCurrentEffect();
        }

        // 버튼 클릭 시 해당 효과에 맞는 슬라이더 값 업데이트
        private void UpdateSliderForCurrentEffect()
        {
            switch (currentEffect)
            {
                case "Exposure":
                    Slider.Value = currentOptions.Exposure;
                    break;
                case "Brightness":
                    Slider.Value = currentOptions.Brightness;
                    break;
                case "Hightlight":
                    Slider.Value = currentOptions.Highlight;
                    break;
                case "Shadow":
                    Slider.Value = currentOptions.Shadow;
                    break;
            }

        }

        // 슬라이더 핸들러
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue != e.OldValue) // 값이 변경되었을 때
            {
                // 현재 선택된 효과에 따라 슬라이더 값을 적용
                switch (currentEffect)
                {
                    case "Exposure":
                        currentOptions.Exposure = (int)e.NewValue; // 'double'을 'int'로 변환
                        break;
                    case "Brightness":
                        currentOptions.Brightness = (int)e.NewValue; // 'double'을 'int'로 변환
                        break;
                    case "Hightlight":
                        currentOptions.Highlight = (int)e.NewValue; // 'double'을 'int'로 변환
                        break;
                    case "Shadow":
                        currentOptions.Shadow = (int)e.NewValue; // 'double'을 'int'로 변환
                        break;
                }

                ApplyAllEffects();

            }
        }

        // 모든 효과를 설정
        private void ApplyAllEffects()
        {
            if (originalImage == null)
                return;

            // 시작점: 원본 이미지
            Mat adjustedImage = originalImage.Clone();

            // 순차적으로 효과를 적용
            adjustedImage = ApplyExposure(adjustedImage, currentOptions.Exposure);
            adjustedImage = ApplyBrightness(adjustedImage, currentOptions.Brightness);
            adjustedImage = ApplyHighlight(adjustedImage, currentOptions.Brightness);
            adjustedImage = ApplyShadow(adjustedImage, currentOptions.Shadow);

            // 결과 이미지를 WPF 이미지 컨트롤에 표시
            ImageDisplay1.Source = MatToBitmapImage(adjustedImage);

            // 변경된 이미지를 현재 이미지로 저장
            currentImage = adjustedImage;
        }

        // 노출 LUT 캐싱 변수
        private readonly Dictionary<int, Mat> exposureLUTCache = new Dictionary<int, Mat>();

        // 노출 LUT 가져오기 (근사 값 캐싱)
        private Mat GetExposureLUT(int exposureValue)
        {
            // 슬라이더 값을 5단위로 근사화
            int approximatedValue = (int)(Math.Round(exposureValue / 5.0) * 5);

            if (!exposureLUTCache.ContainsKey(approximatedValue))
            {
                // LUT 생성 후 캐싱
                exposureLUTCache[approximatedValue] = GenerateExposureLUT(approximatedValue);

                // 캐싱된 LUT가 너무 많아지면 가장 오래된 항목 제거
                if (exposureLUTCache.Count > 20) // 캐싱 개수 제한
                {
                    int oldestKey = exposureLUTCache.Keys.First();
                    exposureLUTCache[oldestKey].Dispose(); // 메모리 해제
                    exposureLUTCache.Remove(oldestKey);
                }
            }
            return exposureLUTCache[approximatedValue];
        }

        // LUT 생성 함수
        private Mat GenerateExposureLUT(int exposureValue)
        {
            // 감마 조정 값 계산
            double gamma = 1.0 - (exposureValue * 0.005); // 감마 범위: 0.95 ~ 1.05

            // LUT 생성
            Mat lut = new Mat(1, 256, MatType.CV_8U);
            for (int i = 0; i < 256; i++)
            {
                double adjustedValue = Math.Pow(i / 255.0, gamma) * 255.0;
                lut.Set(0, i, (byte)Math.Clamp(adjustedValue, 0, 255));
            }
            return lut;
        }

        // 노출 변경 함수
        private Mat ApplyExposure(Mat inputImage, int exposureValue)
        {
            // LUT 가져오기
            Mat lut = GetExposureLUT(exposureValue);

            // LUT를 사용하여 노출 조정
            Mat adjustedImage = new Mat();
            Cv2.LUT(inputImage, lut, adjustedImage);

            return adjustedImage;
        }

        // 밝기 LUT 생성 함수
        private Mat GenerateBrightnessLUT(int brightnessValue)
        {
            Mat lut = new Mat(1, 256, MatType.CV_8U);
            double brightnessAdjustment = brightnessValue * 0.5;

            for (int i = 0; i < 256; i++)
            {
                // 각 픽셀 값에 밝기 조정을 적용
                int adjustedValue = (int)(i + brightnessAdjustment);
                lut.Set(0, i, (byte)Math.Clamp(adjustedValue, 0, 255)); // 0~255로 제한
            }

            return lut;
        }

        // 밝기 변경 함수
        private Mat ApplyBrightness(Mat inputImage, int brightnessValue)
        {
            // LUT 생성
            Mat brightnessLUT = GenerateBrightnessLUT(brightnessValue);

            // LUT 적용
            Mat adjustedImage = new Mat();
            Cv2.LUT(inputImage, brightnessLUT, adjustedImage);

            return adjustedImage;
        }

        // 그림자 변경
        private Mat ApplyShadow(Mat inputImage, int shadowValue)
        {
            // 이미지를 부동 소수점 형식으로 변환
            Mat floatImage = new Mat();
            inputImage.ConvertTo(floatImage, MatType.CV_32F);

            // 채널별 밝기 계산
            Mat[] channels = Cv2.Split(floatImage);
            Mat intensity = new Mat();
            Cv2.AddWeighted(channels[0], 0.333, channels[1], 0.333, 0, intensity);
            Cv2.AddWeighted(intensity, 1.0, channels[2], 0.333, 0, intensity);

            // 히스토그램 평균을 기반으로 임계값 설정
            Scalar mean = Cv2.Mean(intensity);
            double thresholdValue = mean.Val0 * 0.3; // 평균값의 30%를 임계값으로 설정

            // 어두운 영역 마스크 생성
            Mat mask = new Mat();
            Cv2.Threshold(intensity, mask, thresholdValue, 1, ThresholdTypes.BinaryInv);

            // 그림자 강도 조정
            double shadowAdjustment = shadowValue * 0.01;
            for (int i = 0; i < 3; i++)
            {
                Mat shadowChannel = new Mat();
                Cv2.Multiply(mask, shadowAdjustment * intensity, shadowChannel);
                Cv2.Subtract(channels[i], shadowChannel, channels[i]);
                Cv2.Max(channels[i], new Scalar(0), channels[i]); // 값 제한
            }

            // 채널 병합
            Mat shadowImage = new Mat();
            Cv2.Merge(channels, shadowImage);
            shadowImage.ConvertTo(shadowImage, MatType.CV_8U);

            return shadowImage;
        }

        // 하이라이트 변경
        private Mat ApplyHighlight(Mat inputImage, int highlightValue)
        {
            if (inputImage == null)
            {
                throw new ArgumentNullException(nameof(inputImage), "입력된 이미지가 null입니다.");
            }

            // 이미지를 부동 소수점 형식으로 변환
            Mat floatImage = new Mat();
            inputImage.ConvertTo(floatImage, MatType.CV_32F);

            // 밝기 계산 (R, G, B 평균값)
            Mat intensity = new Mat();
            Cv2.CvtColor(floatImage, intensity, ColorConversionCodes.BGR2GRAY);

            // 히스토그램 계산
            int[] histSize = { 256 };
            Rangef[] ranges = { new Rangef(0, 256) };
            Mat hist = new Mat();
            Cv2.CalcHist(new Mat[] { intensity }, new int[] { 0 }, null, hist, 1, histSize, ranges);

            // 히스토그램 누적합 계산
            double totalPixels = intensity.Rows * intensity.Cols;
            double cumulativeSum = 0;
            double highlightThreshold = 255;
            double highlightPercentage = 0.10; // 상위 10%
            for (int i = 255; i >= 0; i--)
            {
                cumulativeSum += hist.At<float>(i);
                if (cumulativeSum / totalPixels >= highlightPercentage)
                {
                    highlightThreshold = i;
                    break;
                }
            }

            // 밝은 영역 마스크 생성
            Mat mask = new Mat();
            Cv2.Threshold(intensity, mask, highlightThreshold, 255, ThresholdTypes.Binary);

            // 하이라이트 강도 조정
            double highlightAdjustment = highlightValue * 0.2; // 강도 증가
            Mat adjustedImage = new Mat();
            Mat[] channels = Cv2.Split(floatImage);

            // 밝은 영역에만 조정
            for (int i = 0; i < 3; i++) // R, G, B 각 채널
            {
                Mat highlightChannel = new Mat();
                Cv2.Multiply(mask, highlightAdjustment, highlightChannel); // 마스크에 강도 적용
                Cv2.Add(channels[i], highlightChannel, channels[i]); // 밝은 영역에 값 추가
                Cv2.Min(channels[i], new Scalar(255), channels[i]); // 클램핑
            }

            // 채널 병합
            Cv2.Merge(channels, adjustedImage);
            adjustedImage.ConvertTo(adjustedImage, MatType.CV_8U);

            return adjustedImage;
        }

    }
}
