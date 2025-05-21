let firebaseConfig = {};

async function initializeFirebase() {
    try {
        const response = await fetch('appsettings.json');
        const config = await response.json();
        firebaseConfig = config.Firebase;
        firebase.initializeApp(firebaseConfig);
    } catch (error) {
        console.error('Error loading Firebase configuration:', error);
    }
}

// Initialize Firebase
initializeFirebase();


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