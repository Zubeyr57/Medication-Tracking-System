package main

import (
	"encoding/json"
	"fmt"
	"net/http"
	"strconv"

	"github.com/gorilla/mux"
)

// Medication model
type Medication struct {
	ID    int      `json:"id"`
	Name  string   `json:"name"`
	Times string   `json:"times"`
	Detail string  `json:"detail"`
	Freq   int     `Json:"freq"`
	CurrDay string `Json:"currDay"`
	NextDay string `Json:"nextDay"`
	TotalPiece int `Json:"totalPiece"`
	UsagePiece int `Json:"usagePiece"`
}

// Global medication slice
var medications []Medication

func main() {
	

	// Router oluştur
	r := mux.NewRouter()

	// Rotaları tanımla
	r.HandleFunc("/medications", getMedications).Methods("GET")
	r.HandleFunc("/medications", createMedication).Methods("POST")
	r.HandleFunc("/medications/{id}", updateMedication).Methods("PUT")
	r.HandleFunc("/medications/{id}", deleteMedication).Methods("DELETE")

	// Sunucuyu başlat
	fmt.Println("Server is running on http://172.16.106.179:8000")
	http.ListenAndServe("172.16.106.179:8000", r)
}

// Tüm ilaçları getir
func getMedications(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(medications)
}

// Yeni ilaç ekle
func createMedication(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	var medication Medication
	json.NewDecoder(r.Body).Decode(&medication)
	medications = append(medications, medication)
	json.NewEncoder(w).Encode(medication)
}

// Mevcut bir ilacı güncelle
func updateMedication(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	params := mux.Vars(r)
	id, _ := strconv.Atoi(params["id"])
	for index, item := range medications {
		if item.ID == id {
			var updatedMedication Medication
			json.NewDecoder(r.Body).Decode(&updatedMedication)
			updatedMedication.ID = id
			medications[index] = updatedMedication
			json.NewEncoder(w).Encode(updatedMedication)
			return
		}
	}
	http.Error(w, "Medication not found", http.StatusNotFound)
}

// Bir ilacı sil
func deleteMedication(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	params := mux.Vars(r)
	id, _ := strconv.Atoi(params["id"])
	for index, item := range medications {
		if item.ID == id {
			medications = append(medications[:index], medications[index+1:]...)
			json.NewEncoder(w).Encode(item)
			return
		}
	}
	http.Error(w, "Medication not found", http.StatusNotFound)
}
