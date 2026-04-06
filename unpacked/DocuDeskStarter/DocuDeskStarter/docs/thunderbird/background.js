async function pingNativeHost() {
  try {
    const response = await browser.runtime.sendNativeMessage("de.docudesk.host", {
      command: "ping"
    });
    console.log("DocuDesk host response", response);
  } catch (error) {
    console.error("Native host call failed", error);
  }
}

browser.runtime.onInstalled.addListener(() => {
  pingNativeHost();
});
