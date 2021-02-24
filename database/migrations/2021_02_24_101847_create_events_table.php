<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

class CreateEventsTable extends Migration
{
    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up()
    {
        Schema::create('events', function (Blueprint $table) {
            $table->id();
            $table->unsignedBigInteger('user_id');
            $table->unsignedBigInteger('location_id');
            $table->unsignedBigInteger('approvedBy_id');
            $table->string('title',150);
            $table->string('description',1500);
            $table->integer('numMinUsers')->default(1);
            $table->integer('numMaxUsers')->default(1);
            $table->integer('minPrice');//si arriven al minim s'aplica aquesta tarifa, sino es divideix a parts iguals el temps entre els assistents
            $table->dateTime('initDate');
            $table->integer('time')->default(1);

            $table->timestamps();
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down()
    {
        Schema::dropIfExists('events');
    }
}
