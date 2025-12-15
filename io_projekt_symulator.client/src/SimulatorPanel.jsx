import { useState, useEffect } from 'react'
import { Plus, Cpu } from "lucide-react";
import { motion } from "framer-motion"
import { AnimatePresence } from "framer-motion";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import Button from "@mui/material/Button";
import DeviceCard from "./components/DeviceCard";
import CreateDeviceModal from "./components/CreateDeviceModal";
import DeviceControlModal from "./components/DeviceControlModal";

function SimulatorPanel() {

    // State for modals and their functions
    const [createModalOpen, setCreateModalOpen] = useState(false);
    const [selectedDevice, setSelectedDevice] = useState(null);
    const queryClient = useQueryClient();

    // Fetch devices function
    const { data: devices = [], isLoading } = useQuery({
        queryKey: ['devices'],
        queryFn: async () => {
            const response = await fetch("/api/devices");
            if (!response.ok) { throw new Error("Failed to fetch devices"); }
            return response.json();
        }
    });

    // Polling effect to refresh device list every 5 seconds
    useEffect(() => {
        // do not poll if modal is open
        if (selectedDevice || createModalOpen) return;
        const id = setInterval(() => {
            queryClient.invalidateQueries({ queryKey: ['devices'] });
        }, 5000);
        return () => clearInterval(id);
    }, [selectedDevice, createModalOpen, queryClient]);


    // Mutations for creating the device type object through API
    const createDeviceMutation = useMutation({
        mutationFn: async (deviceData) => {
            const response = await fetch("/api/devices", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(deviceData),
            });
            if (!response.ok) { throw new Error("Create device failed"); }
            return response.json();
        },

        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["devices"] });
            setCreateModalOpen(false);
        },
    });

    // Mutation for updating the existing device state through API
    const updateDeviceMutation = useMutation({
        mutationFn: async ({ id, data }) => {
            const response = await fetch(`/api/devices/${id}/state`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(data),
            });
            if (!response.ok) { throw new Error("Create update failed"); }
            return response.json();
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['devices'] });
        },
    });

    const handleMalfunctionMutation = useMutation({
        mutationFn: async({ id, state }) => {
            const response = await fetch(`/api/devices/${id}/malfunction`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(state),
            });
            if (!response.ok) { throw new Error("Malfunction toggle failed"); }
            return response.json();
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['devices'] });
        },
    });

    // Mutation for deleting the existing device through API
    const deleteDeviceMutation = useMutation({
        mutationFn: async (id) => {
            const response = await fetch(`/api/devices/${id}`, {
                method: "DELETE",
            });

            if (!response.ok) { throw new Error("Failed to delete"); }
            else if (response.status === 200) return null;
        },

        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: ["devices"] });
        },

        onSuccess: () => {
            setSelectedDevice(null);
        },
    });

    // Handler for device state updates
    const handleDeviceUpdate = (device, newVal) => {
        updateDeviceMutation.mutate({
            id: device.id,
            data: { value: newVal }
        });
    };

    const handleMalfunctionUpdate = (device, state) => {
        handleMalfunctionMutation.mutate({
            id: device.id,
            state: { malfunctioning: state }
        });
    };

    return (
        <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
            {/* Header */}
            <div className="border-b border-slate-200 bg-white/80 backdrop-blur-sm sticky top-0 z-10">
                <div className="max-w-7xl mx-auto px-6 py-6">
                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-3">
                            <div className="w-12 h-12 bg-gradient-to-br from-blue-500 to-cyan-500 rounded-2xl flex items-center justify-center shadow-lg">
                                <Cpu className="w-6 h-6 text-white" />
                            </div>
                            <div>
                                <h1 className="text-2xl font-bold text-slate-900">IoT Device Simulator</h1>
                                <p className="text-sm text-slate-500">Simulate and control virtual devices</p>
                            </div>
                        </div>
                        <Button onClick={() => setCreateModalOpen(true)}
                            className="bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 shadow-lg">
                            <Plus className="w-4 h-4 mr-2" />
                            Add Device
                        </Button>
                    </div>
                </div>
            </div>

            {/* Main Content */}
            <div className="max-w-7xl mx-auto px-6 py-8">
                {isLoading ? (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {[1, 2, 3].map((i) => (
                            <div key={i} className="h-64 bg-white rounded-2xl animate-pulse" />
                        ))}
                    </div>)
                    :
                    devices.length === 0 ? (
                    <div className="text-center py-20">
                        <div className="w-24 h-24 bg-slate-100 rounded-full flex items-center justify-center mx-auto mb-6">
                            <Cpu className="w-12 h-12 text-slate-400" />
                        </div>
                        <h3 className="text-xl font-semibold text-slate-900 mb-2">No devices yet</h3>
                        <p className="text-slate-500 mb-6">Create first virtual IoT device</p>
                        <Button
                            onClick={() => setCreateModalOpen(true)}
                            className="bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700">
                            <Plus className="w-4 h-4 mr-2" />
                            Add Device
                        </Button>
                    </div>) 
                    :
                    (<motion.div
                        layout
                        className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        <AnimatePresence mode="popLayout">
                            {devices.map((device) => (
                                <DeviceCard
                                    key={device.id}
                                    device={device}
                                    onSelect={() => setSelectedDevice(device)}
                                    onUpdate={handleDeviceUpdate}/>)
                            )}
                        </AnimatePresence>
                    </motion.div>)
                }
            </div>

            {/* Modal - open only when createModalOpen == true */}
            <CreateDeviceModal
                open={createModalOpen}
                onClose={() => setCreateModalOpen(false)}
                onCreate={(data) => createDeviceMutation.mutate(data)}
                isCreating={createDeviceMutation.isPending}
            />

            {/* Modal - open only when selectedDevice != null */}
            <DeviceControlModal
                device={selectedDevice}
                open={!!selectedDevice}
                onClose={() => setSelectedDevice(null)}
                onUpdate={handleDeviceUpdate}
                onDelete={() => deleteDeviceMutation.mutate(selectedDevice.id)}
                isUpdating={updateDeviceMutation.isPending}
                isDeleting={deleteDeviceMutation.isPending}
                handleMalfunction={handleMalfunctionUpdate}
            />
        </div>
    )
}

export default SimulatorPanel
