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
using System.Diagnostics.CodeAnalysis;
using System.Printing;
using Rect = OpenCvSharp.Rect;

namespace PhotoEdit
{
    public struct Option
    {
        public int Exposure { get; set; }
        public int Shadow { get; set; }
        public int Brightness { get; set; }
        public int Contrast { get; set; }
        public int Chroma { get; set; }
        public int Highlight { get; set; }
        public int ColorTmp { get; set; }
    }

    public partial class MainWindow : System.Windows.Window
    {
        private Mat originalImage; // 원본 이미지
        private Mat currentImage; // 현재 이미지
        private string currentFilePath; // 현재 파일 경로

        private Option currentOptions = new Option();
        private string currentEffect = "Exposure";
        private string prevEffect = "Exposure";

        // 현재 활성화된 버튼, 효과
        private Button currentActiveButton = null;
        private Button currentOptionButton = null;

        // 크롭, 회전 여부
        private bool isCroped = false;
        private bool isRotate = false;


        public MainWindow()
        {
            InitializeComponent();
            currentActiveButton = AdjustButton; // 기본 활성 버튼 설정
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
                    originalImage = Cv2.ImRead(currentFilePath); // 이미지 읽기
                    currentImage = originalImage.Clone();
                    ImageDisplay1.Source = MatToBitmapImage(currentImage);

                    // 저장 버튼 비활성화
                    SaveButton.Visibility = Visibility.Collapsed;
                    // 구조체 초기화
                    InitializeOptions();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"이미지를 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 설정값 초기화 함수
        private void InitializeOptions()
        {
            currentOptions = new Option
            {
                Exposure = 0,
                Shadow = 0,
                Brightness = 0,
                Contrast = 0,
                Chroma = 0,
                Highlight = 0,
                ColorTmp = 0
            };

            currentEffect = "Exposure"; // 기본 효과 설정
            prevEffect = "Exposure"; // 기본 효과 설정
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAsImage(sender, e);
        }

        // BitmapImage로 변환
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

        // 초기 화면 이미지 불러오기 버튼
        private void LoadImgButton_Click(object sender, RoutedEventArgs e)
        {
            OpenImage(sender, e);

            if (ImageDisplay1.Source != null)
            {
                LoadImgButton.Visibility = Visibility.Collapsed;
            }
        }

        // 효과 버튼
        private void ExposureButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtonBorder((Button)sender, ref currentOptionButton);
            prevEffect = currentEffect;
            currentEffect = "Exposure";
            UpdateSliderForCurrentEffect();
            UpdateEffectDisplay();
        }

        private void ShadowButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtonBorder((Button)sender, ref currentOptionButton);
            prevEffect = currentEffect;
            currentEffect = "Shadow";
            UpdateSliderForCurrentEffect();
            UpdateEffectDisplay();
        }

        private void BrightnessButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtonBorder((Button)sender, ref currentOptionButton);
            prevEffect = currentEffect;
            currentEffect = "Brightness";
            UpdateSliderForCurrentEffect();
            UpdateEffectDisplay();
        }

        private void ContrastButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtonBorder((Button)sender, ref currentOptionButton);
            prevEffect = currentEffect;
            currentEffect = "Contrast";
            UpdateSliderForCurrentEffect();
            UpdateEffectDisplay();
        }

        private void HighlightButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtonBorder((Button)sender, ref currentOptionButton);
            prevEffect = currentEffect;
            currentEffect = "Highlight";
            UpdateSliderForCurrentEffect();
            UpdateEffectDisplay();
        }

        private void ChromaButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtonBorder((Button)sender, ref currentOptionButton);
            prevEffect = currentEffect;
            currentEffect = "Chroma";
            UpdateSliderForCurrentEffect();
            UpdateEffectDisplay();
        }

        private void ColorTmpButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtonBorder((Button)sender, ref currentOptionButton);
            prevEffect = currentEffect;
            currentEffect = "ColorTmp";
            UpdateSliderForCurrentEffect();
            UpdateEffectDisplay();
        }

        private void FlipButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentImage == null)
            {
                return;
            }

            Mat flippedImage = new Mat();
            Cv2.Flip(currentImage, flippedImage, FlipMode.X);

            currentImage = flippedImage.Clone();
            ImageDisplay1.Source = MatToBitmapImage(flippedImage);

            // 원본 업데이트
            Cv2.Flip(originalImage, flippedImage, FlipMode.X);
            originalImage = flippedImage.Clone();

            isRotate = true;
        }

        private void MirrorButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentImage == null)
            {
                return;
            }

            Mat flippedImage = new Mat();
            Cv2.Flip(currentImage, flippedImage, FlipMode.Y);

            currentImage = flippedImage.Clone();
            ImageDisplay1.Source = MatToBitmapImage(flippedImage);

            // 원본 업데이트
            Cv2.Flip(originalImage, flippedImage, FlipMode.Y);
            originalImage = flippedImage.Clone();

            isRotate = true;
        }

        private void RotateRightButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentImage == null)
            {
                return;
            }

            Mat rotatedImage = new Mat();
            Cv2.Rotate(currentImage, rotatedImage, RotateFlags.Rotate90Clockwise);

            currentImage = rotatedImage.Clone();
            ImageDisplay1.Source = MatToBitmapImage(rotatedImage);

            // 원본 업데이트
            Cv2.Rotate(originalImage, rotatedImage, RotateFlags.Rotate90Clockwise);
            originalImage = rotatedImage.Clone();

            isRotate = true;
        }

        private void RotateLeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentImage == null)
            {
                return;
            }

            Mat rotatedImage = new Mat();
            Cv2.Rotate(currentImage, rotatedImage, RotateFlags.Rotate90Counterclockwise);

            currentImage = rotatedImage.Clone();
            ImageDisplay1.Source = MatToBitmapImage(rotatedImage);

            // 원본 업데이트
            Cv2.Rotate(originalImage, rotatedImage, RotateFlags.Rotate90Counterclockwise);
            originalImage = rotatedImage.Clone();

            isRotate = true;
        }
        
        // 버튼 클릭 시 해당 효과에 맞는 슬라이더 값 업데이트
        private void UpdateSliderForCurrentEffect()
        {
            switch (currentEffect)
            {
                case "Exposure":
                    Slider.Value = currentOptions.Exposure;
                    break;
                case "Shadow":
                    Slider.Value = currentOptions.Shadow;
                    break;
                case "Brightness":
                    Slider.Value = currentOptions.Brightness;
                    break;
                case "Contrast":
                    Slider.Value = currentOptions.Contrast;
                    break;
                case "Chroma":
                    Slider.Value = currentOptions.Chroma;
                    break;
                case "Hightlight":
                    Slider.Value = currentOptions.Highlight;
                    break;
                case "ColorTmp":
                    Slider.Value = currentOptions.ColorTmp;
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
                    case "Shadow":
                        currentOptions.Shadow = (int)e.NewValue; // 'double'을 'int'로 변환
                        break;
                    case "Brightness":
                        currentOptions.Brightness = (int)e.NewValue; // 'double'을 'int'로 변환
                        break;
                    case "Contrast":
                        currentOptions.Contrast = (int)e.NewValue; // 'double'을 'int'로 변환
                        break;
                    case "Chroma":
                        currentOptions.Chroma = (int)e.NewValue; // 'double'을 'int'로 변환
                        break;

                    case "Hightlight":
                        currentOptions.Highlight = (int)e.NewValue; // 'double'을 'int'로 변환
                        break;

                    case "ColorTmp":
                        currentOptions.ColorTmp = (int)e.NewValue; // 'double'을 'int'로 변환
                        break;
                    case "Rotate":
                        ApplyRotation(e.NewValue);
                        return;
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
            adjustedImage = ApplyShadow(adjustedImage, currentOptions.Shadow);
            adjustedImage = ApplyContrast(adjustedImage, currentOptions.Contrast);
            adjustedImage = ApplyBrightness(adjustedImage, currentOptions.Brightness);
            adjustedImage = ApplyChroma(adjustedImage, currentOptions.Chroma);
            adjustedImage = ApplyHighlight(adjustedImage, currentOptions.Brightness);
            adjustedImage = ApplyColorTmp(adjustedImage, currentOptions.ColorTmp);

            // 결과 이미지를 WPF 이미지 컨트롤에 표시
            ImageDisplay1.Source = MatToBitmapImage(adjustedImage);

            // 변경된 이미지를 현재 이미지로 저장
            currentImage = adjustedImage;
        }

        // 노출 변경 (감마값 기반)
        private Mat ApplyExposure(Mat inputImage, int exposureValue)
        {
            // 감마 조정 값 계산
            double gamma = 1.0 - (exposureValue * 0.005); // 감마값


            // LUT(룩업 테이블) 생성
            Mat lut = new Mat(1, 256, MatType.CV_8U);
            for (int i = 0; i < 256; i++)
            {
                double normalizedValue = i / 255.0; // 0~255 값을 0~1로 정규화
                double adjustedValue = Math.Pow(normalizedValue, gamma) * 255.0; // 감마 보정
                lut.Set(0, i, (byte)Math.Clamp(adjustedValue, 0, 255)); // 0~255로 제한
            }

            // LUT를 사용하여 노출 조정
            Mat adjustedImage = new Mat();
            Cv2.LUT(inputImage, lut, adjustedImage);

            return adjustedImage;
        }

        // 밝기 변경
        private Mat ApplyBrightness(Mat inputImage, int brightnessValue)
        {
            // 밝기 조정 값 계산
            double brightnessAdjustment = brightnessValue * 0.5;

            // 부동 소수점 형식으로 변환 (연산을 위해)
            Mat floatImage = new Mat();
            inputImage.ConvertTo(floatImage, MatType.CV_32F);

            // 밝기 조정: 모든 픽셀에 동일한 값을 더함
            Mat adjustedImage = new Mat();
            Cv2.Add(floatImage, new Scalar(brightnessAdjustment, brightnessAdjustment, brightnessAdjustment), adjustedImage);

            // 값이 0~255 범위를 초과하지 않도록 클램핑
            Cv2.Min(adjustedImage, new Scalar(255, 255, 255), adjustedImage);
            Cv2.Max(adjustedImage, new Scalar(0, 0, 0), adjustedImage);

            // 다시 8비트 형식으로 변환
            adjustedImage.ConvertTo(adjustedImage, MatType.CV_8U);

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
            double thresholdValue = mean.Val0 * 0.3; // 임계값 설정

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
            double highlightAdjustment = highlightValue * 0.2; 
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

        // 대비 변경
        private Mat ApplyContrast(Mat inputImage, int contrastValue)
        {
            // 대비 조정 값 계산
            double contrastFactor = 1.0 + (contrastValue / 250.0);
            contrastFactor = Math.Clamp(contrastFactor, 0.0, 3.0); // 대비 조정 범위 제한

            // 이미지 평균값 계산 (중립 점 설정)
            Scalar mean = Cv2.Mean(inputImage);

            // 대비 조정
            Mat adjustedImage = new Mat();
            inputImage.ConvertTo(adjustedImage, MatType.CV_32F); // 부동 소수점 변환
            Cv2.Subtract(adjustedImage, new Scalar(mean.Val0, mean.Val1, mean.Val2), adjustedImage); // 평균값을 기준으로 이동
            Cv2.Multiply(adjustedImage, contrastFactor, adjustedImage); // 대비 확장 또는 축소
            Cv2.Add(adjustedImage, new Scalar(mean.Val0, mean.Val1, mean.Val2), adjustedImage); // 원래 위치로 복원

            // 값 클램핑 및 다시 8비트 변환
            adjustedImage.ConvertTo(adjustedImage, MatType.CV_8U, 1.0, 0);

            return adjustedImage;
        }

        // 채도변경
        private Mat ApplyChroma(Mat inputImage, int chromaValue)
        {
            // 채도 조정 비율 계산
            double chromaFactor = 1.0 + (chromaValue / 100.0); 
            chromaFactor = Math.Clamp(chromaFactor, 0.0, 2.0); // 채도 조정 범위 제한

            // BGR 이미지를 HSV 색 공간으로 변환
            Mat hsvImage = new Mat();
            Cv2.CvtColor(inputImage, hsvImage, ColorConversionCodes.BGR2HSV);

            // HSV 이미지를 분리 (H, S, V)
            Mat[] hsvChannels = Cv2.Split(hsvImage);

            // S (채도) 채널 조정
            Cv2.Multiply(hsvChannels[1], chromaFactor, hsvChannels[1]); // 채도에 chromaFactor를 곱함
            Cv2.Min(hsvChannels[1], new Scalar(255), hsvChannels[1]);   // 값 제한 (0 ~ 255)

            // 조정된 채널 병합
            Cv2.Merge(hsvChannels, hsvImage);

            // HSV 이미지를 BGR로 다시 변환
            Mat adjustedImage = new Mat();
            Cv2.CvtColor(hsvImage, adjustedImage, ColorConversionCodes.HSV2BGR);

            return adjustedImage;
        }

        // 색온도 변경
        private Mat ApplyColorTmp(Mat inputImage, int tempValue)
        {
            if (inputImage == null)
            {
                throw new ArgumentNullException(nameof(inputImage), "입력된 이미지가 null입니다.");
            }

            // 슬라이더 값 (-100 ~ 100)을 이용해 RGB 가중치 계산
            double redAdjustment = 1.0 + (tempValue * 0.005); // 온도를 높이면 빨간색 강화
            double blueAdjustment = 1.0 - (tempValue * 0.005); // 온도를 낮추면 파란색 강화

            redAdjustment = Math.Clamp(redAdjustment, 0.0, 1.0);
            blueAdjustment = Math.Clamp(blueAdjustment, 0.0, 1.0);

            // BGR 채널 분리
            Mat[] channels = Cv2.Split(inputImage);

            // 빨간색 채널 조정
            Cv2.Multiply(channels[2], redAdjustment, channels[2]); // Red 채널
            Cv2.Min(channels[2], new Scalar(255), channels[2]);    // 값 제한

            // 파란색 채널 조정
            Cv2.Multiply(channels[0], blueAdjustment, channels[0]); // Blue 채널
            Cv2.Min(channels[0], new Scalar(255), channels[0]);     // 값 제한

            // 채널 병합
            Mat adjustedImage = new Mat();
            Cv2.Merge(channels, adjustedImage);

            return adjustedImage;
        }

        // 편집 모드 활성화
        private void AcitvateEditScreen()
        {
            // 윈도우 배경을 검은색으로 변경
            Main.Background = new SolidColorBrush(Colors.Black);

            // 이미지 배경을 검은색으로 변경
            ImageSheet.Background = new SolidColorBrush(Colors.Black);

            // 이미지 확대 (예: 1.5배)
            ImageScaleTransform.ScaleX = 1.05;
            ImageScaleTransform.ScaleY = 1.05;
        }

        // 슬라이더 초기 설정
        private void SetSliderValue()
        {
            // 슬라이더 값 변경 시 이벤트 일시적으로 비활성화
            Slider.ValueChanged -= Slider_ValueChanged;

            if (currentEffect == "Rotate")
            {
                Slider.Minimum = -45;
                Slider.Maximum = 45;
                Slider.Value = 0;
                Slider.Height = 350;
            }
            else
            {
                Slider.Minimum = -100;
                Slider.Maximum = 100;

                // currentEffect에 따라 슬라이더 값 설정
                Slider.Value = currentEffect switch
                {
                    "Exposure" => currentOptions.Exposure,
                    "Shadow" => currentOptions.Shadow,
                    "Brightness" => currentOptions.Brightness,
                    "Contrast" => currentOptions.Contrast,
                    "Chroma" => currentOptions.Chroma,
                    "Highlight" => currentOptions.Highlight,
                    "ColorTmp" => currentOptions.ColorTmp,
                };

                Slider.Height = 550;
            }

            // 슬라이더 값 변경 시 이벤트 다시 활성화
            Slider.ValueChanged += Slider_ValueChanged;
        }


        // 표시할 기능 설정
        private void SetVisible()
        {
            FileBox.Visibility = Visibility.Collapsed;
            CompleteButton.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Collapsed;

            if (currentActiveButton == AdjustButton)
            {
                // 활성화
                Slider.Visibility = Visibility.Visible;
                tools.Visibility = Visibility.Visible;
                EffectDisplay.Visibility = Visibility.Visible;
                UpdateEffectDisplay();

                // 숨기기
                CropCanvas.Visibility = Visibility.Collapsed;
                SelectionRectangle.Visibility = Visibility.Collapsed;
                rotation.Visibility = Visibility.Collapsed;
            }
            else
            {
                // 활성화
                rotation.Visibility = Visibility.Visible;
                Slider.Visibility = Visibility.Visible;

                // 숨기기
                EffectDisplay.Visibility = Visibility.Collapsed;
                tools.Visibility = Visibility.Collapsed;
                ShowSelectionRectangle();
            }
        }

        // 조절 버튼
        private void AdjustButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentImage == null)
            {
                return;
            }

            UpdateButtonBorder((Button)sender, ref currentActiveButton);

            if(currentEffect == "Exposure")
            {
                UpdateButtonBorder(ExposureButton, ref currentOptionButton);
            }
            currentEffect = prevEffect;
 
            SetSliderValue();

            SetVisible();
            AcitvateEditScreen();

            UpdateEffectDisplay();
            UpdateSliderForCurrentEffect();
        }


        // 자르기 및 회전 버튼
        private void TransferButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentImage == null)
            {
                return;
            }

            UpdateButtonBorder((Button)sender, ref currentActiveButton);

            if (currentEffect != "Rotate")
            {
                prevEffect = currentEffect;
            }

            currentEffect = "Rotate";


            SetSliderValue();
            SetVisible();

            AcitvateEditScreen();
        }

        // 슬라이더 회전 처리
        private void ApplyRotation(double angle)
        {
            if (originalImage == null)
            {
                return;
            }

            // 원본 이미지의 중심 좌표 계산
            Point2f center = new Point2f(originalImage.Width / 2.0f, originalImage.Height / 2.0f);

            // 회전 매트릭스 생성
            Mat rotationMatrix = Cv2.GetRotationMatrix2D(center, angle, 1.0);

            Mat rotatedImage = new Mat();

            // WarpAffine으로 이미지 회전
            Cv2.WarpAffine(originalImage, rotatedImage, rotationMatrix, originalImage.Size());

            // 원본 이미지를 업데이트
            originalImage = rotatedImage;

            // 결과 표시할 이미지 회전
            Cv2.WarpAffine(currentImage, rotatedImage, rotationMatrix, currentImage.Size());
            ImageDisplay1.Source = MatToBitmapImage(rotatedImage);

        }

        // 화면 사이즈 얻기
        private System.Windows.Size GetDisplayedImageSize()
        {
            if (ImageDisplay1.Source is BitmapSource bitmapSource)
            {
                // 이미지 원본 크기
                double originalWidth = bitmapSource.PixelWidth;
                double originalHeight = bitmapSource.PixelHeight;

                // Image 컨트롤의 크기
                double containerWidth = ImageDisplay1.ActualWidth;
                double containerHeight = ImageDisplay1.ActualHeight;

                // Uniform 스케일 계산
                double widthScale = containerWidth / originalWidth;
                double heightScale = containerHeight / originalHeight;

                // Uniform 특성을 따른 최소 스케일 적용
                double scale = Math.Min(widthScale, heightScale);

                // 표시 크기 계산
                double displayedWidth = originalWidth * scale;
                double displayedHeight = originalHeight * scale;

                return new System.Windows.Size(displayedWidth, displayedHeight);
            }

            return new System.Windows.Size(0, 0); // 이미지가 없을 경우
        }

        // 자를 사각형 표시
        private void ShowSelectionRectangle()
        {
            // 표시된 이미지의 실제 크기 가져오기
            System.Windows.Size displayedSize = GetDisplayedImageSize();

            if (displayedSize.Width <= 0 || displayedSize.Height <= 0)
            {
                return;
            }

            // 캔버스 크기 설정
            CropCanvas.Width = displayedSize.Width;
            CropCanvas.Height = displayedSize.Height;

            // 사각형 크기 설정
            SelectionRectangle.Width = displayedSize.Width * 1.05;
            SelectionRectangle.Height = displayedSize.Height * 1.05;

            // 사각형을 캔버스 중앙에 정렬
            Canvas.SetLeft(SelectionRectangle, (CropCanvas.Width - SelectionRectangle.Width) / 2);
            Canvas.SetTop(SelectionRectangle, (CropCanvas.Height - SelectionRectangle.Height) / 2);

            // 캔버스 및 사각형 표시
            CropCanvas.Visibility = Visibility.Visible;
            SelectionRectangle.Visibility = Visibility.Visible;
        }

        // 사각형 크기 조절
        private void CropCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            const double zoomFactor = 0.03; // 크기 조정 비율
            double delta = e.Delta > 0 ? 1 + zoomFactor : 1 - zoomFactor; // 휠 방향에 따라 확대 또는 축소

            // 표시된 이미지의 크기 가져오기
            System.Windows.Size displayedSize = GetDisplayedImageSize();
            if (displayedSize.Width <= 0 || displayedSize.Height <= 0)
            {
                return;
            }

            // 크기 제한 계산
            const double minSize = 100; // 최소 크기
            double maxWidth = displayedSize.Width * 1.05; // 최대 너비
            double maxHeight = displayedSize.Height * 1.05; // 최대 높이

            // 새로운 사각형 크기 계산
            double newWidth = SelectionRectangle.Width * delta;
            double newHeight = SelectionRectangle.Height * delta;

            // 크기 제한 적용
            newWidth = Math.Clamp(newWidth, minSize, maxWidth);
            newHeight = Math.Clamp(newHeight, minSize, maxHeight);

            // 선택 사각형 크기 업데이트
            SelectionRectangle.Width = newWidth;
            SelectionRectangle.Height = newHeight;

            // 선택 사각형을 캔버스 중앙으로 재정렬
            Canvas.SetLeft(SelectionRectangle, (CropCanvas.Width - SelectionRectangle.Width) / 2);
            Canvas.SetTop(SelectionRectangle, (CropCanvas.Height - SelectionRectangle.Height) / 2);

            isCroped = true;
        }

        // 사진 자르기
        private Mat CropImageByCanvas()
        {
            if (currentImage == null || CropCanvas.Visibility != Visibility.Visible)
            {
                return null;
            }

            // 표시된 이미지의 크기 가져오기
            System.Windows.Size displayedSize = GetDisplayedImageSize();
            if (displayedSize.Width <= 0 || displayedSize.Height <= 0)
            {
                return null;
            }

            // 캔버스의 사각형 위치 및 크기 가져오기
            double rectX = Canvas.GetLeft(SelectionRectangle);
            double rectY = Canvas.GetTop(SelectionRectangle);
            double rectWidth = SelectionRectangle.Width;
            double rectHeight = SelectionRectangle.Height;

            // 이미지 크기 대비 표시된 크기의 비율 계산
            double widthRatio = originalImage.Width / displayedSize.Width;
            double heightRatio = originalImage.Height / displayedSize.Height;

            // 사각형 좌표를 원본 이미지 좌표로 변환
            int cropX = (int)((rectX - (CropCanvas.Width - displayedSize.Width) / 2) * widthRatio);
            int cropY = (int)((rectY - (CropCanvas.Height - displayedSize.Height) / 2) * heightRatio);
            int cropWidth = (int)(rectWidth * widthRatio);
            int cropHeight = (int)(rectHeight * heightRatio);

            cropX = Math.Clamp(cropX, 0, originalImage.Width - 1);
            cropY = Math.Clamp(cropY, 0, originalImage.Height - 1);
            cropWidth = Math.Clamp(cropWidth, 1, originalImage.Width - cropX);
            cropHeight = Math.Clamp(cropHeight, 1, originalImage.Height - cropY);

            Rect cropRect = new Rect(cropX, cropY, cropWidth, cropHeight);

            // 이미지 자르기
            Mat croppedImage = new Mat(originalImage, cropRect);

            isCroped = true;

            return croppedImage;
        }

        // 완료 버튼
        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            // 자르기 했다면
            if (isCroped)
            {
                // 자르기 영역 계산 및 이미지 자르기
                Mat croppedImage = CropImageByCanvas();
                if (croppedImage != null)
                {
                    // 자른 이미지를 UI에 반영
                    currentImage = croppedImage;
                    originalImage = croppedImage.Clone();
                    ImageDisplay1.Source = MatToBitmapImage(croppedImage);

                    // 자르기 캔버스 숨기기
                    CropCanvas.Visibility = Visibility.Collapsed;
                    isCroped = false; // 상태 초기화 
                }
                else
                {
                    isCroped = false; // 상태 초기화 
                    return;
                }
            }
            else if (isRotate)
            {
                isRotate = false;
            }
            else
            {
                // 원본을 갱신하고 초기 상태로 복원
                ResetEditScreen();
                isCroped = false; // 상태 초기화 
            }

        }

        // 편집 화면 초기화
        private void ResetEditScreen()
        {
            // 배경 색상 초기화
            Main.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#606060")); ; // 기본 배경색으로 복원
            ImageSheet.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));

            // 이미지 확대 상태 초기화
            ImageScaleTransform.ScaleX = 1.0;
            ImageScaleTransform.ScaleY = 1.0;

            // 슬라이더와 UI 요소 숨기기
            Slider.Visibility = Visibility.Collapsed;
            tools.Visibility = Visibility.Collapsed;
            rotation.Visibility = Visibility.Collapsed;
            CompleteButton.Visibility = Visibility.Collapsed;


            // 캔버스 숨기기
            CropCanvas.Visibility = Visibility.Collapsed;
            SelectionRectangle.Visibility = Visibility.Collapsed;

            // 텍스트 숨기기
            EffectDisplay.Visibility = Visibility.Collapsed; 

            // 활성 버튼 초기화
            if (currentActiveButton != null)
            {
                currentActiveButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF673AB7")); // 기본 색상
            }
            currentActiveButton = null;

            // 저장 버튼 활성화
            SaveButton.Visibility = Visibility.Visible;

            // 파일박스 활성화
            FileBox.Visibility = Visibility.Visible;
        }

        // 버튼 활성화 
        private void UpdateButtonBorder(Button clickedButton, ref Button currentButton)
        {
            // 이전 활성 버튼 테두리를 기본 색으로 복원
            if (currentButton != null)
            {
                currentButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF673AB7")); // 기본색
            }

            // 클릭된 버튼의 테두리를 초록색으로 설정
            clickedButton.BorderBrush = new SolidColorBrush(Colors.LimeGreen);

            // 현재 활성 버튼 업데이트
            currentButton = clickedButton;
        }

        // 현재 선택 효과 표시
        private void UpdateEffectDisplay()
        {
            string effectDisplayText = currentEffect switch
            {
                "Exposure" => "노출",
                "Shadow" => "그림자",
                "Brightness" => "밝기",
                "Contrast" => "대비",
                "Chroma" => "채도",
                "Highlight" => "하이라이트",
                "ColorTmp" => "색온도",
                "Rotate" => "회전",  
            };

            EffectDisplay.Content = effectDisplayText;
        }
    }    
}
