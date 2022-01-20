var App =
    {
        data() {
            return {
              
                errors: [],
                status: ""
            }
        },
        methods:
            {
                doLoginSMS: function (token) {
                    var self = this;
                 
                    this.status = "sending";
                    
                    fetch("/api/account/sendSMSCode", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                             
                                recaptchaToken: token
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {

                            console.log(res);
                            
                            self.status = "done";
                            
                            if (res.success) {
                               window.location = "/Account/SMSLogin?returnUrl=" + returnUrl;
                            } else
                                self.errors.push({message: res.message});
                        });

                },
                loginSMS: function () {

                    var self = this;
                    
                    this.errors = [];
                    formLoginSMS.classList.remove("was-validated");
                   
                    
                    var res = formLoginSMS.checkValidity();
                    if (res) {
                        grecaptcha.ready(function () {
                            grecaptcha.execute(recaptcha, {action: 'submit'}).then(function (token) {
                                self.doLoginSMS(token);
                            });
                        });
                    } else {
                        formLoginSMS.classList.add("was-validated");
                      

                        var list = formLoginSMS.querySelectorAll(":invalid");

                        list.forEach((element) => {
                            this.errors.push({ message : element.validationMessage, id : element.id});
                        });

                    }
                }
            }
    };

Vue.createApp(App).mount('#app')