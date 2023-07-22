console.log("Hello, World!");
console.log("Wazzup?");

var a = "Delayed";

scheduler.delay(2000, () => {  // callbacks will not be called before script exection of main script has been finished
    console.log(a); 
});

for (let index = 0; index <  99999999; index++) {
    ;;
}