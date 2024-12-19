using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpRaise.Models.Converters;

namespace UpRaise.Models.BitPay
{

    //
    //https://bitpay.com/api/
    //
    public class InvoicePaymentNotification
    {
        [JsonProperty(PropertyName = "event")]
        public InvoicePaymentNotificationEvent Event
        {
            get; set;
        }

        [JsonProperty(PropertyName = "data")]
        public InvoicePaymentNotificationData Data
        {
            get; set;
        }

    }

    public class InvoicePaymentNotificationEvent
    {
        [JsonProperty(PropertyName = "code")]
        public int Code
        {
            get; set;
        }


        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get; set;
        }
    }

    public class InvoicePaymentNotificationData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get; set;
        }

        [JsonProperty(PropertyName = "url")]
        public string Url
        {
            get; set;
        }

        [JsonProperty(PropertyName = "posData")]
        public string PosData
        {
            get; set;
        }

        [JsonProperty(PropertyName = "status")]
        public string Status
        {
            get; set;
        }


        [JsonProperty(PropertyName = "price")]
        public decimal Price
        {
            get; set;
        }

        [JsonProperty(PropertyName = "currency")]
        public string Currency
        {
            get; set;
        }

        [JsonConverter(typeof(DateTimeJsonConverter))]
        [JsonProperty(PropertyName = "invoiceTime")]
        public DateTimeOffset InvoiceTime
        {
            get; set;
        }

        [JsonConverter(typeof(DateTimeJsonConverter))]
        [JsonProperty(PropertyName = "expirationTime")]
        public DateTimeOffset ExpirationTime
        {
            get; set;
        }


        [JsonConverter(typeof(DateTimeJsonConverter))]
        [JsonProperty(PropertyName = "currentTime")]
        public DateTimeOffset CurrentTime
        {
            get; set;
        }

        [JsonProperty(PropertyName = "exceptionStatus")]
        public JToken ExceptionStatus
        {
            get; set;
        }

        [JsonProperty(PropertyName = "buyerFields")]
        public JObject BuyerFields
        {
            get; set;
        }

        [JsonProperty(PropertyName = "paymentSubtotals")]
        public Dictionary<string, decimal> PaymentSubtotals
        {
            get; set;
        }

        [JsonProperty(PropertyName = "paymentTotals")]
        public Dictionary<string, decimal> PaymentTotals
        {
            get; set;
        }

        [JsonProperty(PropertyName = "exchangeRates")]
        public Dictionary<string, Dictionary<string, decimal>> ExchangeRates
        {
            get; set;
        }

        [JsonProperty(PropertyName = "amountPaid")]
        public decimal AmountPaid
        {
            get; set;
        }

        [JsonProperty(PropertyName = "orderId")]
        public string OrderId
        {
            get; set;
        }

        [JsonProperty(PropertyName = "transactionCurrency")]
        public string TransactionCurrency
        {
            get; set;
        }
    }
}
