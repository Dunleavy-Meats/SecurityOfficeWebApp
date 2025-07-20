
const firebaseConfig = {
    apiKey: "AIzaSyCg2RGzOWlGY55WcMktt7Vg1l5r30ece4Y",
    authDomain: "security-system-c0b0e.firebaseapp.com",
    projectId: "security-system-c0b0e",
    storageBucket: "security-system-c0b0e.firebasestorage.app",
    messagingSenderId: "565564609130",
    appId: "1:565564609130:web:fce3021f128b764e3d7013",
    measurementId: "G-4SQ8R3FM4M"
};

// Initialize Firebase
firebase.initializeApp(firebaseConfig);


window.firebaseSignInWithEmailAndPassword = async (email, password) => {
    try {
        const userCredential = await firebase.auth().signInWithEmailAndPassword(email, password);
        return userCredential.user.uid;
    } catch (error) {
        console.error("Sign in error:", error);
        throw error;
    }
};

window.firebaseCreateUserWithEmailAndPassword = async (email, password) => {
    try {
        const userCredential = await firebase.auth().createUserWithEmailAndPassword(email, password);
        return userCredential.user.uid;
    } catch (error) {
        console.error("Sign up error:", error);
        throw error;
    }
};

window.firebaseSignOut = async () => {
    await firebase.auth().signOut();
};


window.firebaseGetCurrentUser = async () => {
    return new Promise((resolve) => {
        firebase.auth().onAuthStateChanged(user => {
            if (user) {
                resolve({
                    uid: user.uid,
                    email: user.email,
                    displayName: user.displayName,
                    token: user.auth.currentUser.accessToken
                });
            } else {
                resolve(null);
            }
        });
    });
};

window.firebaseSignOut = async () => {
    await firebase.auth().signOut();
};


window.showPdfPrintDialog = function (base64Data) {
    // Create a blob from the base64 data
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: 'application/pdf' });

    // Create object URL
    const url = URL.createObjectURL(blob);

    // Open in new window and print
    const printWindow = window.open(url, '_blank');
    if (printWindow) {
        printWindow.addEventListener('load', function () {
            try {
                printWindow.print();
                // Cleanup
                /*                   setTimeout(() => {
                                       printWindow.close();
                                       URL.revokeObjectURL(url);
                                   }, 10000000);*/
            } catch (e) {
                console.error('Print failed:', e);
                printWindow.close();
                URL.revokeObjectURL(url);
            }
        });
    }
};


window.downloadPDF = function (filename, byteBase64) {
    console.log("Attempting to download PDF:", filename);
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/pdf;base64," + byteBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};