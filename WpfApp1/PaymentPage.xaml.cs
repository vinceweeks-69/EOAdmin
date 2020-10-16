using Newtonsoft.Json;
using SharedData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using ViewModels.ControllerModels;
using ViewModels.DataModels;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for PaymentPage.xaml
    /// </summary>
    public partial class PaymentPage : EOStackPage
    {
        WorkOrderResponse currentWorkOrder = new WorkOrderResponse();

        PersonAndAddressDTO Customer = new PersonAndAddressDTO(); 

        public PaymentPage()
        {
            InitializeComponent();

            ObservableCollection<KeyValuePair<int, string>> paymentType = new ObservableCollection<KeyValuePair<int, string>>();

            paymentType.Add(new KeyValuePair<int, string>(0, "Cash"));
            paymentType.Add(new KeyValuePair<int, string>(1, "Check"));
            paymentType.Add(new KeyValuePair<int, string>(2, "Credit Card"));

            PaymentTypeComboBox.ItemsSource = paymentType;
            PaymentTypeComboBox.SelectedIndex = 0;

            ObservableCollection<KeyValuePair<int, string>> discountType = new ObservableCollection<KeyValuePair<int, string>>();

            discountType.Add(new KeyValuePair<int, string>(0, "None"));
            discountType.Add(new KeyValuePair<int, string>(1, "Percent"));
            discountType.Add(new KeyValuePair<int, string>(2, "Manual"));

            DiscountTypeComboBox.ItemsSource = discountType;
            DiscountTypeComboBox.SelectedIndex = 0;
            
            DiscountAmountLabel.Visibility = Visibility.Hidden;
            DiscountAmountTextBox.Visibility = Visibility.Hidden;

            GiftCardNumberLabel.Visibility = Visibility.Hidden;
            GiftCardNumberTextBox.Visibility = Visibility.Hidden;

            GiftCardAmountLabel.Visibility = Visibility.Hidden;
            GiftCardAmountTextBox.Visibility = Visibility.Hidden;

            CCGrid.Visibility = Visibility.Hidden;
        }

        public PaymentPage(WorkOrderResponse  workOrder, PersonDTO customer) : this()
        {
            currentWorkOrder = workOrder;
            Customer.Person = customer;
            GetWorkOrderDetail();
        }

        public async void GetWorkOrderDetail()
        {

            WorkOrderResponse request = new WorkOrderResponse();
            request.WorkOrderList = currentWorkOrder.WorkOrderList;
            request.NotInInventory = currentWorkOrder.NotInInventory;
            request.Arrangements = currentWorkOrder.Arrangements;

            ((App)App.Current).PostRequest<WorkOrderResponse, GetWorkOrderSalesDetailResponse>("GetWorkOrderDetail", request).ContinueWith(a => DetailLoaded(a.Result));
        }

        void DetailLoaded(GetWorkOrderSalesDetailResponse response)
        {
            if (response.Success)
            {
                Dispatcher.Invoke(() =>
                {
                    SubTotalTextBox.Text = response.SubTotal.ToString("N2");

                    TaxTextBox.Text = response.Tax.ToString("N2");

                    TotalTextBox.Text = WorkOrderTotal();
                });
            }
        }

        public string WorkOrderTotal()
        {
            decimal total = Convert.ToDecimal(SubTotalTextBox.Text);
            decimal tax = 0.0M;

            if (!String.IsNullOrEmpty(TaxTextBox.Text))
            {
                tax = Convert.ToDecimal(TaxTextBox.Text);
                total += tax; 
            }

            if (!String.IsNullOrEmpty(DiscountAmountTextBox.Text))
            {
                if (DiscountTypeComboBox.SelectedIndex == 1)  //percent 
                {
                    decimal temp = Convert.ToDecimal(DiscountAmountTextBox.Text);
                    if (temp != 0 && temp < 100)
                    {
                        total -= total * (temp / 100);
                    }
                }
                else
                {
                    total -= Convert.ToDecimal(DiscountAmountTextBox.Text);
                }
            }

            if (!String.IsNullOrEmpty(GiftCardAmountTextBox.Text))
            {
                total -= Convert.ToDecimal(GiftCardAmountTextBox.Text);
            }

            return  total.ToString("N2");
        }

        private void PageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void PaymentTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            if(cb != null)
            {
                if(cb.SelectedIndex == 2)
                {
                    CCGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    CCGrid.Visibility = Visibility.Hidden;
                }
            }
        }

        string MessageFormatter(List<string> msgs)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string msg in msgs)
            {
                sb.Append(msg + "\n");
            }

            return sb.ToString();
        }

        private async void Pay_Click(object sender, RoutedEventArgs e)
        {
            //send record for payment

            //success? show message - go back to Work Order page

            //failure? show message stay on this page

            Pay.IsEnabled = false;

            bool proceedWithSave = true;

            string ccConfirm = String.Empty;

            if (PaymentTypeComboBox.SelectedIndex == 2)
            {
                CreditCard cc = new CreditCard()
                {
                    Cvc = CVVTextBox.Text,
                    HolderName = NameOnCardTextBox.Text,
                    Numbers = CardNumberTextBox.Text,
                    Month = ExpMonthTextBox.Text,
                    Year = ExpYearTextBox.Text
                };

                List<string> msgs = cc.VerifyCreditCardInfo();

                if (msgs.Count == 0)
                {
                    PaymentResponse paymentResponse = await PayWithCC(cc);

                    ccConfirm = paymentResponse.ccConfirm;

                    proceedWithSave = paymentResponse.success;

                    if(paymentResponse.Messages.Count > 0)
                    {
                        MessageBox.Show("Error", MessageFormatter(paymentResponse.Messages["Stripe"]), MessageBoxButton.OK);
                    }
                }
                else
                {
                    proceedWithSave = false;
                    MessageBox.Show("Error", MessageFormatter(msgs), MessageBoxButton.OK);
                }
            }
            
            if(proceedWithSave)
            {
                bool paymentSaved = await SavePaymentRecord(ccConfirm);

                if (paymentSaved)
                {
                    MessageBox.Show("Payment Successful", "Success", MessageBoxButton.OK);
                    MainWindow wnd = Application.Current.MainWindow as MainWindow;
                    if(wnd.PageIsOnStack(typeof(WorkOrderPage)))
                    {
                        EOStackPage parentPage = wnd.GetEOStackPage(typeof(WorkOrderPage));

                        if(parentPage != null)
                        {
                            WorkOrderMessage msg = new WorkOrderMessage();
                            msg.WorkOrderPaid = true;
                            parentPage.LoadWorkOrderData(msg);
                        }
                    }

                    wnd.OnBackClick(this);
                }
                else
                {
                    MessageBox.Show("There was a problem saving the payment record", "Error",  MessageBoxButton.OK);
                }
            }

            Pay.IsEnabled = true;
        }

        private async Task<PaymentResponse> PayWithCC(CreditCard cc)
        {
            PaymentResponse paymentResponse = new PaymentResponse();

            try
            { 
                StripeServices stripe = new StripeServices();

                CardValidate ccValidate = stripe.CardToToken(cc).Result;

                if (!String.IsNullOrEmpty(ccValidate.ccConfirm))
                {
                    cc.token = ccValidate.ccConfirm;

                    string strMoneyValue = TotalTextBox.Text;
                    decimal decimalMoneyValue = decimal.Parse(strMoneyValue, NumberStyles.Currency);
                    paymentResponse  =  await MakeStripePayment(cc, decimalMoneyValue);
                    //paymentResponse = p.Result;
                    paymentResponse.ccConfirm = ccValidate.ccConfirm;
                }
                else
                {
                    paymentResponse.Messages.Add("Stripe", ccValidate.ErrorMessages);
                }
            }
            catch (Exception ex)
            {
                //CANNOT save cc data to db
                Exception ex2 = new Exception("PayWithCC", ex);
                ((App)App.Current).LogError(ex2.Message, "Card holder name is " + NameOnCardTextBox.Text);
            }

            return paymentResponse;
        }

        public async Task<PaymentResponse> MakeStripePayment(CreditCard creditCard, decimal salePrice)
        {
            PaymentResponse response = new PaymentResponse();

            try
            {
                CardInput cc = new CardInput();

                cc.nameonCard = creditCard.HolderName;
                ////testing 4242424242424242
                cc.cardNumber = creditCard.Numbers;
                cc.expirationDate = new DateTime(Convert.ToInt32(creditCard.Year), Convert.ToInt32(creditCard.Month), 1).ToShortDateString();
                cc.cvc = creditCard.Cvc;
                cc.amount = salePrice;
                cc.customerName = creditCard.HolderName;

                string hash = JsonConvert.SerializeObject(cc);

                PaymentRequest request = new PaymentRequest();

                Random rnd = new Random();
                request.test = rnd.Next(32, 64);

                request.payload = Encryption.EncryptStringToBytes(hash, Encryption.GetBytes(Encryption.StatsOne(request.test)), Encryption.GetBytes(Encryption.StatsTwo(request.test)));

                response = await ((App)App.Current).PostRequest<PaymentRequest, PaymentResponse>("MakeStripePayment", request);
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception("MakeStripePayment", ex);
                ((App)App.Current).LogError(ex2.Message, "Card holder name is " + creditCard.HolderName);
            }

            return response;
        }

        private async Task<bool> SavePaymentRecord(string ccConfirm)
        {
            WorkOrderPaymentDTO workOrderPayment = GetWorkOrderPaymentDTO(ccConfirm);

            ApiResponse response = await ((App)App.Current).PostRequest<WorkOrderPaymentDTO, ApiResponse>("AddWorkOrderPayment", workOrderPayment);

            if (response.Success)
            {
                EmailReceipt(workOrderPayment);
            }

            return response.Success;
        }

        private WorkOrderPaymentDTO GetWorkOrderPaymentDTO(string ccConfirm)
        {
            WorkOrderPaymentDTO workOrderPayment = new WorkOrderPaymentDTO();
            decimal workValue = 0;
            workOrderPayment.WorkOrderId = currentWorkOrder.WorkOrder.WorkOrderId;
            decimal.TryParse(TotalTextBox.Text, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out workValue);
            workOrderPayment.WorkOrderPaymentAmount = workValue;
            workValue = 0;
            workOrderPayment.WorkOrderPaymentType = PaymentTypeComboBox.SelectedIndex;
            workOrderPayment.WorkOrderPaymentCreditCardConfirmation = ccConfirm;
            decimal.TryParse(TaxTextBox.Text, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out workValue);
            workOrderPayment.WorkOrderPaymentTax = workValue;
            workOrderPayment.DiscountType = DiscountTypeComboBox.SelectedIndex;

            if (workOrderPayment.DiscountType > 0)
            {
                workValue = 0;
                decimal.TryParse(DiscountAmountTextBox.Text, out workValue);
                workOrderPayment.DiscountAmount = workValue;
            }

            return workOrderPayment;
        }

        private void EmailReceipt(WorkOrderPaymentDTO workOrderPayment)
        {
            try
            {
                if (Customer.Person.person_id > 0)
                {
                    EmailHelpers emailHelper = new EmailHelpers();

                    EOMailMessage mailMessage = new EOMailMessage();

                    string emailHtml = String.Empty;

                    if (!String.IsNullOrEmpty(Customer.Person.email))
                    {
                        emailHtml = emailHelper.ComposeReceipt(currentWorkOrder, workOrderPayment);

                        mailMessage = new EOMailMessage("service@elegantorchids.com", Customer.Person.email, "Elegant Orchids Receipt", emailHtml, "Orchids@5185");
                    }
                    else //let EO know the customer needs to add an email address
                    {
                        emailHtml = emailHelper.ComposeMissingEmail(Customer);

                        mailMessage = new EOMailMessage("service@elegantorchids.com", "information@elegantorchids.com", "Missing Customer Email", emailHtml, "Orchids@5185");
                    }

                    Email.SendEmail(mailMessage);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Application.Current.MainWindow as MainWindow;
            wnd.OnBackClick(this);
        }

        private void DiscountTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            if(cb != null)
            {
                if(cb.SelectedIndex > 0)
                {
                    DiscountAmountLabel.Visibility = Visibility.Visible;
                    DiscountAmountTextBox.Visibility = Visibility.Visible;
                }
                else
                {
                    DiscountAmountLabel.Visibility = Visibility.Hidden;
                    DiscountAmountTextBox.Visibility = Visibility.Hidden;
                }
            }
        }

        private void GiftCardCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            if (cb != null)
            {
                if (cb.SelectedIndex > 0)
                {
                    GiftCardNumberLabel.Visibility = Visibility.Visible;
                    GiftCardNumberTextBox.Visibility = Visibility.Visible;

                    GiftCardAmountLabel.Visibility = Visibility.Visible;
                    GiftCardAmountTextBox.Visibility = Visibility.Visible;
                }
                else
                {
                    GiftCardNumberLabel.Visibility = Visibility.Hidden;
                    GiftCardNumberTextBox.Visibility = Visibility.Hidden;

                    GiftCardAmountLabel.Visibility = Visibility.Hidden;
                    GiftCardAmountTextBox.Visibility = Visibility.Hidden;
                }
            }
        }

        private void DiscountAmountTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TotalTextBox.Text = WorkOrderTotal();
        }

        private void GiftCardAmountTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TotalTextBox.Text = WorkOrderTotal();
        }
    }
}
